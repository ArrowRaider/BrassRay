using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using AutoMapper;
using YamlDotNet.Serialization;

namespace BrassRay.RayTracer.IO
{
    public static class Serialization
    {
        private static readonly Regex RgbExp = new(@"^rgb\s*\[\s*(\S+),\s*(\S+),\s*(\S+)\s*\]$");
        private static readonly Regex VecExp = new(@"^vec\s*\[\s*(\S+),\s*(\S+),\s*(\S+)\s*\]$");

        private static float DegToRad(float x) => x * MathF.PI / 180.0f;

        private static Vector3 DegToRad(Vector3 x) =>
            new(x.X * MathF.PI / 180.0f, x.Y * MathF.PI / 180.0f, x.Z * MathF.PI / 180.0f);

        private static Vector3 RadToDeg(Vector3 x) =>
            new(x.X / MathF.PI * 180.0f, x.Y / MathF.PI * 180.0f, x.Z / MathF.PI * 180.0f);
        
        public static Scene ReadScene(string yamlString)
        {
            var deserializer = new DeserializerBuilder().Build();
            var dto = deserializer.Deserialize<Dictionary<object, object>>(yamlString);
            var dict = ConvertDictionaries(dto);
            var colorModel = GetColorModel(dict);
            
            var baseProfile = new BaseProfile(colorModel);
            var transformsProfile = new OuterTransformProfile(baseProfile);
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(baseProfile);
                cfg.AddProfile(transformsProfile);
            });
            var mapper = configuration.CreateMapper();
            if (dict.TryGetValue("Transforms", out var rawTransforms))
                mapper.Map<List<TransformHolder>>(rawTransforms);

            var samplersProfile = new OuterSamplerProfile(baseProfile);
            configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(baseProfile);
                cfg.AddProfile(transformsProfile);
                cfg.AddProfile(samplersProfile);
            });
            mapper = configuration.CreateMapper();
            if (dict.TryGetValue("Samplers", out var rawSamplers))
                mapper.Map<List<SamplerHolder>>(rawSamplers);

            var materialsProfile = new OuterMaterialProfile(baseProfile, samplersProfile);
            configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(baseProfile);
                cfg.AddProfile(transformsProfile);
                cfg.AddProfile(samplersProfile);
                cfg.AddProfile(materialsProfile);
            });
            mapper = configuration.CreateMapper();
            if (dict.TryGetValue("Materials", out var rawMaterials))
                mapper.Map<List<MaterialHolder>>(rawMaterials);
            var environmentDto = mapper.Map<SamplerHolder>(dict[nameof(SceneDto.Environment)]);
            var cameraDto = mapper.Map<CameraHolder>(dict[nameof(SceneDto.Camera)]);
            var drawablesDto = mapper.Map<List<DrawableHolder>>(dict[nameof(SceneDto.Drawables)]);
            var sceneDto = new SceneDto
                { Camera = cameraDto, ColorModel = colorModel, Drawables = drawablesDto, Environment = environmentDto };

            configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(baseProfile);
                cfg.AddProfile(new CameraProfile());
                cfg.AddProfile(new DrawableProfile());
                cfg.AddProfile(new MaterialProfile());
                cfg.AddProfile(new SamplerProfile());
                cfg.AddProfile(new TransformProfile());
                cfg.AddProfile(new SceneProfile());
            });
            mapper = configuration.CreateMapper();

            return mapper.Map<Scene>(sceneDto);
            
        }

        public static string WriteScene(Scene scene)
        {
            throw new NotImplementedException();
        }

        private static ColorModel GetColorModel(Dictionary<string, object> dict)
        {
            if (!dict.TryGetValue(nameof(Scene.ColorModel), out var rawColorModel)) return new ColorModel();
            var configuration = new MapperConfiguration(cfg => { });
            var mapper = configuration.CreateMapper();
            return mapper.Map<ColorModel>(rawColorModel);
        }

        private static Dictionary<string, object> ConvertDictionaries(Dictionary<object, object> d) =>
            d.ToDictionary(p => p.Key.ToString(),
                p => p.Value switch
                {
                    Dictionary<object, object> dNext => ConvertDictionaries(dNext),
                    List<object> list => list.Select(el =>
                        el is Dictionary<object, object> elDNext ? ConvertDictionaries(elDNext) : el).ToList(),
                    _ => p.Value
                });

        private class BaseProfile : Profile
        {
            public BaseProfile(ColorModel colorModel)
            {
                CreateMap<Vector3, Rgb>().ConvertUsing(s => colorModel.VectorToRgb(s));
                CreateMap<Rgb, Vector3>().ConvertUsing(s => colorModel.RgbToVector(s));

                CreateMap<string, Vector3>().ConvertUsing((s, d, c) =>
                {
                    Match match;
                    if ((match = RgbExp.Match(s)).Success &&
                        float.TryParse(match.Groups[1].Value, out var r) &&
                        float.TryParse(match.Groups[2].Value, out var g) &&
                        float.TryParse(match.Groups[3].Value, out var b))
                    {
                        var item = new Rgb(r, g, b);
                        return c.Mapper.Map(item, d);
                    }

                    if ((match = VecExp.Match(s)).Success &&
                        float.TryParse(match.Groups[1].Value, out var x) &&
                        float.TryParse(match.Groups[2].Value, out var y) &&
                        float.TryParse(match.Groups[3].Value, out var z))
                        return new Vector3(x, y, z);
                    throw new InvalidDataException();
                });

                CreateMap<string, Rgb>().ConvertUsing((s, d, c) =>
                {
                    Match match;
                    if ((match = RgbExp.Match(s)).Success &&
                        float.TryParse(match.Groups[1].Value, out var r) &&
                        float.TryParse(match.Groups[2].Value, out var g) &&
                        float.TryParse(match.Groups[3].Value, out var b))
                        return new Rgb(r, g, b);
                    if ((match = VecExp.Match(s)).Success &&
                        float.TryParse(match.Groups[1].Value, out var x) &&
                        float.TryParse(match.Groups[2].Value, out var y) &&
                        float.TryParse(match.Groups[3].Value, out var z))
                    {
                        var item = new Vector3(x, y, z);
                        return c.Mapper.Map(item, d);
                    }

                    throw new InvalidDataException();
                });
            }

            public static bool IsVector3(string s) => VecExp.IsMatch(s);
            public static bool IsRgb(string s) => RgbExp.IsMatch(s);
        }

        private class OuterTransformProfile : Profile
        {
            public OuterTransformProfile(BaseProfile baseProfile)
            {
                var innerMapper = new MapperConfiguration(cfg => { cfg.AddProfile(baseProfile); }).CreateMapper();

                CreateMap<Dictionary<string, object>, TransformHolder>()
                    .ConstructUsing(s => innerMapper.Map<TransformHolder>(s)).AfterMap((_, d, c) =>
                    {
                        BuildDictionary(Enumerable.Repeat(d, 1), Lookup);
                    });

                CreateMap<string, List<TransformHolder>>().ConstructUsing(s => Lookup[s]);
            }

            public Dictionary<string, List<TransformHolder>> Lookup { get; } = new();

            private static void BuildDictionary(
                IEnumerable<TransformHolder> holders,
                IDictionary<string, List<TransformHolder>> dict,
                ImmutableStack<TransformHolder> current = null)
            {
                current ??= ImmutableStack<TransformHolder>.Empty;

                foreach (var holder in holders)
                {
                    var next = current.Push(holder);
                    if (!string.IsNullOrWhiteSpace(holder.Name))
                        dict.TryAdd(holder.Name, next.ToList());
                    BuildDictionary(holder.Children ?? Enumerable.Empty<TransformHolder>(), dict, next);
                }
            }
        }

        private class OuterSamplerProfile : Profile
        {
            public OuterSamplerProfile(BaseProfile baseProfile)
            {

                var innerMapper = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(baseProfile);
                    cfg.CreateMap<string, SamplerHolder>().ConstructUsing((s, c) =>
                        Lookup.TryGetValue(s, out var sampler)
                            ? sampler
                            : new SamplerHolder
                                { SolidSampler = new SolidSamplerDto { Color = c.Mapper.Map<Rgb>(s) } });
                }).CreateMapper();

                CreateMap<Dictionary<string, object>, SamplerHolder>()
                    .ConstructUsing(s => innerMapper.Map<SamplerHolder>(s))
                    .AfterMap((_, d) => BuildDictionary(d, Lookup));

                CreateMap<string, SamplerHolder>().ConstructUsing((s, c) =>
                    Lookup.TryGetValue(s, out var sampler)
                        ? sampler
                        : new SamplerHolder { SolidSampler = new SolidSamplerDto { Color = c.Mapper.Map<Rgb>(s) } });
            }

            private static void BuildDictionary(SamplerHolder holder, IDictionary<string, SamplerHolder> dict)
            {
                if (holder is null) return;
                if (!string.IsNullOrWhiteSpace(holder.Name))
                    dict.TryAdd(holder.Name, holder);
                BuildDictionary(holder.CheckerSampler?.Color1, dict);
                BuildDictionary(holder.CheckerSampler?.Color2, dict);
                BuildDictionary(holder.RainbowSampler?.XColor, dict);
                BuildDictionary(holder.RainbowSampler?.YColor, dict);
                BuildDictionary(holder.RainbowSampler?.ZColor, dict);
                BuildDictionary(holder.SkySampler?.HighColor, dict);
                BuildDictionary(holder.SkySampler?.LowColor, dict);
                BuildDictionary(holder.SkySampler?.SunColor, dict);
            }

            public Dictionary<string, SamplerHolder> Lookup { get; } = new();
        }

        private class OuterMaterialProfile : Profile
        {
            public OuterMaterialProfile(BaseProfile baseProfile, OuterSamplerProfile outerSamplerProfile)
            {
                var innerMapper = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(baseProfile);
                    cfg.AddProfile(outerSamplerProfile);
                    cfg.CreateMap<string, MaterialHolder>().ConstructUsing(s => Lookup[s]);
                }).CreateMapper();

                CreateMap<Dictionary<string, object>, MaterialHolder>()
                    .ConstructUsing(s => innerMapper.Map<MaterialHolder>(s))
                    .AfterMap((_, d) => BuildDictionary(d, Lookup));

                CreateMap<string, MaterialHolder>().ConstructUsing(s => Lookup[s]);
            }

            private static void BuildDictionary(MaterialHolder holder, IDictionary<string, MaterialHolder> dict)
            {
                if (holder is null) return;
                if (!string.IsNullOrWhiteSpace(holder.Name))
                    dict.TryAdd(holder.Name, holder);
                BuildDictionary(holder.SchlickMaterial?.High, dict);
                BuildDictionary(holder.SchlickMaterial?.Low, dict);
            }

            public Dictionary<string, MaterialHolder> Lookup { get; } = new();
        }

        private class CameraProfile : Profile
        {
            public CameraProfile()
            {
                CreateMap<CameraDto, Camera>()
                    .Include<TargetCameraDto, TargetCamera>()
                    .Include<OrthographicCameraDto, OrthographicCamera>()
                    .Include<SphericalCameraDto, SphericalCamera>();
                CreateMap<TargetCameraDto, TargetCamera>();
                CreateMap<OrthographicCameraDto, OrthographicCamera>();
                CreateMap<SphericalCameraDto, SphericalCamera>();
                CreateMap<CameraHolder, CameraDto>().ConstructUsing(s =>
                    s.OrthographicCamera ?? s.SphericalCamera ?? (CameraDto)s.TargetCamera);
                CreateMap<CameraHolder, Camera>()
                    .ConvertUsing((s, d, c) => c.Mapper.Map(c.Mapper.Map<CameraDto>(s), d));
            }
        }

        private class DrawableProfile : Profile
        {
            public DrawableProfile()
            {
                CreateMap<DrawableDto, Drawable>()
                    .Include<InfinitePlaneDto, InfinitePlane>()
                    .Include<BoxDto, Box>()
                    .Include<SphereDto, Sphere>();
                CreateMap<InfinitePlaneDto, InfinitePlane>();
                CreateMap<BoxDto, Box>();
                CreateMap<SphereDto, Sphere>();
                CreateMap<DrawableHolder, DrawableDto>()
                    .ConstructUsing(s => s.Box ?? s.InfinitePlane ?? (DrawableDto)s.Sphere);
                CreateMap<DrawableHolder, Drawable>()
                    .ConvertUsing((s, d, c) => c.Mapper.Map(c.Mapper.Map<DrawableDto>(s), d));
            }
        }

        private class MaterialProfile : Profile
        {
            public MaterialProfile()
            {
                CreateMap<MaterialDto, Material>()
                    .Include<EmissiveMaterialDto, EmissiveMaterial>()
                    .Include<FastDiffuseMaterialDto, FastDiffuseMaterial>()
                    .Include<LambertianMaterialDto, LambertianMaterial>()
                    .Include<ReflectMaterialDto, ReflectMaterial>()
                    .Include<RefractMaterialDto, RefractMaterial>()
                    .Include<SchlickMaterialDto, SchlickMaterial>();
                CreateMap<EmissiveMaterialDto, EmissiveMaterial>();
                CreateMap<FastDiffuseMaterialDto, FastDiffuseMaterial>();
                CreateMap<LambertianMaterialDto, LambertianMaterial>();
                CreateMap<ReflectMaterialDto, ReflectMaterial>();
                CreateMap<RefractMaterialDto, RefractMaterial>();
                CreateMap<SchlickMaterialDto, SchlickMaterial>();
                CreateMap<MaterialHolder, MaterialDto>().ConstructUsing(s =>
                    s.EmissiveMaterial ?? s.FastDiffuseMaterial ?? s.LambertianMaterial ??
                    s.ReflectMaterial ?? s.RefractMaterial ?? (MaterialDto)s.SchlickMaterial);
                CreateMap<MaterialHolder, Material>()
                    .ConvertUsing((s, d, c) => c.Mapper.Map(c.Mapper.Map<MaterialDto>(s), d));
            }
        }

        private class SamplerProfile : Profile
        {
            public SamplerProfile()
            {
                CreateMap<SamplerDto, Sampler>()
                    .Include<SolidSamplerDto, SolidSampler>()
                    .Include<CheckerSamplerDto, CheckerSampler>()
                    .Include<SkySamplerDto, SkySampler>()
                    .Include<RainbowSamplerDto, RainbowSampler>();
                CreateMap<SolidSamplerDto, SolidSampler>();
                CreateMap<CheckerSamplerDto, CheckerSampler>();
                CreateMap<SkySamplerDto, SkySampler>();
                CreateMap<RainbowSamplerDto, RainbowSampler>();
                CreateMap<SamplerHolder, SamplerDto>().ConstructUsing(s =>
                    s.CheckerSampler ?? s.RainbowSampler ?? s.SkySampler ?? (SamplerDto)s.SolidSampler);
                CreateMap<SamplerHolder, Sampler>()
                    .ConvertUsing((s, d, c) => c.Mapper.Map(c.Mapper.Map<SamplerDto>(s), d));
            }
        }

        private class TransformProfile : Profile
        {
            public TransformProfile()
            {
                CreateMap<TransformDto, Matrix4x4>().ConstructUsing((s, _) => s switch
                {
                    RotateTransformDto x when x.Center.HasValue =>
                        Matrix4x4.CreateTranslation(-x.Center.Value) *
                        Matrix4x4.CreateFromYawPitchRoll(DegToRad(x.Rotation.Y), DegToRad(x.Rotation.X),
                            DegToRad(x.Rotation.Z)) *
                        Matrix4x4.CreateTranslation(x.Center.Value),
                    RotateTransformDto x => Matrix4x4.CreateFromYawPitchRoll(DegToRad(x.Rotation.Y), DegToRad(x.Rotation.X),
                        DegToRad(x.Rotation.Z)),
                    ScaleTransformDto x when x.Center.HasValue => Matrix4x4.CreateScale(x.Scale, x.Center.Value),
                    ScaleTransformDto x => Matrix4x4.CreateScale(x.Scale),
                    TranslateTransformDto x => Matrix4x4.CreateTranslation(x.Offset),
                    QuaternionTransformDto x when x.Center.HasValue =>
                        Matrix4x4.CreateTranslation(-x.Center.Value) *
                        Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(x.Axis, DegToRad(x.Angle))) *
                        Matrix4x4.CreateTranslation(x.Center.Value),
                    QuaternionTransformDto x =>
                        Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(x.Axis, DegToRad(x.Angle))),
                    MatrixTransformDto x => new Matrix4x4(x.M11, x.M12, x.M13, x.M14, x.M21, x.M22, x.M23, x.M24,
                        x.M31,
                        x.M32, x.M33, x.M34, x.M41, x.M42, x.M43, x.M44),
                    _ => throw new InvalidOperationException()
                });
                CreateMap<TransformHolder, TransformDto>().ConstructUsing(s =>
                    s.MatrixTransform ?? s.QuaternionTransform ??
                    s.RotateTransform ?? s.ScaleTransform ?? (TransformDto)s.TranslateTransform);
                CreateMap<List<TransformHolder>, Matrix4x4>()
                    .ConvertUsing((s, _, c) =>
                    {
                        if (s == null || s.Count == 0) return Matrix4x4.Identity;
                        var dtos = c.Mapper.Map<List<TransformDto>>(s);
                        var matrices = c.Mapper.Map<List<Matrix4x4>>(dtos);
                        return matrices.Aggregate((left, right) => left * right);
                    });
            }
        }

        private class SceneProfile : Profile
        {
            public SceneProfile()
            {
                CreateMap<SceneDto, Scene>();
            }
        }
    }
}