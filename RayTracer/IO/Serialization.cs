using System;
using System.Collections.Generic;
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
        private static readonly Regex RgbExp = new (@"^rgb\s*\[\s*(\S+),\s*(\S+),\s*(\S+)\s*\]$");
        private static readonly Regex VecExp = new (@"^vec\s*\[\s*(\S+),\s*(\S+),\s*(\S+)\s*\]$");

        private static float DegToRad(float x) => x * MathF.PI / 180.0f;

        private static Vector3 DegToRad(Vector3 x) =>
            new(x.X * MathF.PI / 180.0f, x.Y * MathF.PI / 180.0f, x.Z * MathF.PI / 180.0f);
        private static Vector3 RadToDeg(Vector3 x) =>
            new(x.X / MathF.PI * 180.0f, x.Y / MathF.PI * 180.0f, x.Z / MathF.PI * 180.0f);

        // This mess returns a mapping between concrete-space and dto-space
        private static IMapper CreateMapper(ColorModel colorModel)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Scene, SceneDto>()
                    .ForMember(d => d.Materials,
                        o => o.MapFrom((s, _) => s.Drawables.Select(x => x.Material).Distinct()))
                    .ForMember(d => d.Transforms, o => o.Ignore())
                    .AfterMap((s, d, c) =>
                    {
                        var dict = new Dictionary<Matrix4x4, MatrixTransformDto>();
                        foreach (var (sd, dd) in s.Drawables.Zip(d.Drawables, (sd, dd) => (sd, dd)))
                        {
                            if (sd.Transform.IsIdentity) continue;
                            if (!dict.TryGetValue(sd.Transform, out var dto))
                                dict.Add(sd.Transform, dto = c.Mapper.Map<MatrixTransformDto>(sd.Transform));
                            if (dd.Box != null) dd.Box.Transform = dto.Name;
                            else if (dd.InfinitePlane != null) dd.InfinitePlane.Transform = dto.Name;
                            else if (dd.Sphere != null) dd.Sphere.Transform = dto.Name;
                        }

                        d.Transforms = new List<TransformHolder>();
                        c.Mapper.Map(dict.Values.ToList(), d.Transforms);
                    })
                    .ReverseMap()
                    .AfterMap((s, d, c) =>
                    {
                        var materials = c.Mapper.Map<List<Material>>(s.Materials);
                        var drawableDtos = c.Mapper.Map<List<DrawableDto>>(s.Drawables);
                        var materialDict = materials.ToDictionary(m => m.Name);
                        var matrixDicts = BuildMatrixDict(s.Transforms, Matrix4x4.Identity, c);
                        foreach (var (sd, dd) in drawableDtos.Zip(d.Drawables, (sd, dd) => (sd, dd)))
                        {
                            dd.Material = materialDict[sd.Material];
                            if (!string.IsNullOrWhiteSpace(sd.Transform))
                                dd.Transform = matrixDicts[sd.Transform];
                        }
                    });

                cfg.CreateMap<Environment, EnvironmentDto>().ReverseMap();

                cfg.CreateMap<Drawable, DrawableDto>()
                    .ForMember(d => d.Material, o => o.MapFrom(s => s.Material.Name))
                    .ForMember(d => d.Transform, o => o.Ignore())
                    .Include<InfinitePlane, InfinitePlaneDto>()
                    .Include<Box, BoxDto>()
                    .Include<Sphere, SphereDto>()
                    .ReverseMap().ForMember(d => d.Material, o => o.Ignore())
                    .ForMember(d => d.Transform, o => o.Ignore());
                cfg.CreateMap<InfinitePlane, InfinitePlaneDto>().ReverseMap();
                cfg.CreateMap<Box, BoxDto>().ReverseMap();
                cfg.CreateMap<Sphere, SphereDto>().ReverseMap();
                cfg.CreateMap<Drawable, DrawableHolder>()
                    .ForMember(d => d.InfinitePlane, o => o.MapFrom(s => s as InfinitePlane))
                    .ForMember(d => d.Box, o => o.MapFrom(s => s as Box))
                    .ForMember(d => d.Sphere, o => o.MapFrom(s => s as Sphere))
                    .ReverseMap().ConvertUsing((s, d, c) =>
                    {
                        var item = c.Mapper.Map<DrawableDto>(s);
                        return c.Mapper.Map(item, d);
                    });
                cfg.CreateMap<DrawableHolder, DrawableDto>()
                    .ConvertUsing((s, _) => s.InfinitePlane ?? s.Box ?? (DrawableDto)s.Sphere);

                cfg.CreateMap<Material, MaterialDto>()
                    .Include<EmissiveMaterial, EmissiveMaterialDto>()
                    .Include<FastDiffuseMaterial, FastDiffuseMaterialDto>()
                    .Include<LambertianMaterial, LambertianMaterialDto>()
                    .Include<ReflectMaterial, ReflectMaterialDto>()
                    .Include<RefractMaterial, RefractMaterialDto>()
                    .Include<SchlickMaterial, SchlickMaterialDto>()
                    .ReverseMap();
                cfg.CreateMap<EmissiveMaterial, EmissiveMaterialDto>().ReverseMap();
                cfg.CreateMap<FastDiffuseMaterial, FastDiffuseMaterialDto>().ReverseMap();
                cfg.CreateMap<LambertianMaterial, LambertianMaterialDto>().ReverseMap();
                cfg.CreateMap<ReflectMaterial, ReflectMaterialDto>().ReverseMap();
                cfg.CreateMap<RefractMaterial, RefractMaterialDto>().ReverseMap();
                cfg.CreateMap<SchlickMaterial, SchlickMaterialDto>().ReverseMap();
                cfg.CreateMap<Material, MaterialHolder>()
                    .ForMember(d => d.EmissiveMaterial, o => o.MapFrom(s => s as EmissiveMaterial))
                    .ForMember(d => d.FastDiffuseMaterial, o => o.MapFrom(s => s as FastDiffuseMaterial))
                    .ForMember(d => d.LambertianMaterial, o => o.MapFrom(s => s as LambertianMaterial))
                    .ForMember(d => d.ReflectMaterial, o => o.MapFrom(s => s as ReflectMaterial))
                    .ForMember(d => d.RefractMaterial, o => o.MapFrom(s => s as RefractMaterial))
                    .ForMember(d => d.SchlickMaterial, o => o.MapFrom(s => s as SchlickMaterial))
                    .ReverseMap().ConvertUsing((s, d, c) =>
                    {
                        var item = s.EmissiveMaterial ?? s.FastDiffuseMaterial ?? s.LambertianMaterial ??
                            s.ReflectMaterial ?? s.RefractMaterial ?? (MaterialDto)s.SchlickMaterial;
                        return c.Mapper.Map(item, d);
                    });

                cfg.CreateMap<Camera, CameraDto>()
                    .Include<TargetCamera, TargetCameraDto>()
                    .Include<OrthographicCamera, OrthographicCameraDto>()
                    .Include<SphericalCamera, SphericalCameraDto>().ReverseMap();
                cfg.CreateMap<TargetCamera, TargetCameraDto>().ReverseMap();
                cfg.CreateMap<OrthographicCamera, OrthographicCameraDto>().ReverseMap();
                cfg.CreateMap<SphericalCamera, SphericalCameraDto>()
                    .ForMember(d => d.Rotation, o => o.MapFrom(s => RadToDeg(s.Rotation)))
                    .ReverseMap().ForMember(d => d.Rotation, o => o.MapFrom(s => DegToRad(s.Rotation)));
                cfg.CreateMap<Camera, CameraHolder>()
                    .ForMember(d => d.TargetCamera, o => o.MapFrom(s => s as TargetCamera))
                    .ForMember(d => d.OrthographicCamera, o => o.MapFrom(s => s as OrthographicCamera))
                    .ForMember(d => d.SphericalCamera, o => o.MapFrom(s => s as SphericalCamera))
                    .ReverseMap().ConvertUsing((s, d, c) =>
                    {
                        var item = s.TargetCamera ?? s.OrthographicCamera ?? (CameraDto)s.SphericalCamera;
                        return c.Mapper.Map(item, d);
                    });

                cfg.CreateMap<TransformHolder, TransformDto>().ConvertUsing((s, _) =>
                    s.RotateTransform ?? s.ScaleTransform ??
                    s.TranslateTransform ?? s.QuaternionTransform ?? (TransformDto)s.MatrixTransform);

                // TODO: Decompose transform matrices
                cfg.CreateMap<Matrix4x4, MatrixTransformDto>()
                    .ForMember(d => d.Children, o => o.Ignore())
                    .ForMember(d => d.Name, o => o.MapFrom(_ => Guid.NewGuid().ToString()));
                cfg.CreateMap<MatrixTransformDto, TransformHolder>()
                    .ForMember(d => d.MatrixTransform, o => o.MapFrom(d => d))
                    .ForMember(d => d.QuaternionTransform, o => o.Ignore())
                    .ForMember(d => d.RotateTransform, o => o.Ignore())
                    .ForMember(d => d.ScaleTransform, o => o.Ignore())
                    .ForMember(d => d.TranslateTransform, o => o.Ignore());
                cfg.CreateMap<Matrix4x4, TransformHolder>()
                    .ConvertUsing((s, d, c) =>
                    {
                        var dto = c.Mapper.Map<MatrixTransformDto>(s);
                        return c.Mapper.Map(dto, d);
                    });
                cfg.CreateMap<TransformHolder, Matrix4x4>()
                    .ConvertUsing((s, d, c) =>
                    {
                        if (s == null) return Matrix4x4.Identity;
                        var dto = c.Mapper.Map<TransformDto>(s);
                        return c.Mapper.Map(dto, d);
                    });

                cfg.CreateMap<List<TransformHolder>, Matrix4x4>()
                    .ConvertUsing((s, d, c) =>
                    {
                        if (s == null || s.Count == 0) return Matrix4x4.Identity;
                        var dtos = c.Mapper.Map<List<TransformDto>>(s);
                        var matrices = c.Mapper.Map<List<Matrix4x4>>(dtos);
                        return matrices.Aggregate((left, right) => left * right);
                    });
                cfg.CreateMap<Matrix4x4, List<TransformHolder>> ()
                    .ConvertUsing((s, d, c) =>
                    {
                        throw new NotImplementedException();
                    });

                cfg.CreateMap<Sampler, SamplerDto>()
                    .Include<SolidSampler, SolidSamplerDto>()
                    .Include<CheckerSampler, CheckerSamplerDto>()
                    .Include<SkySampler, SkySamplerDto>()
                    .Include<RainbowSampler, RainbowSamplerDto>().ReverseMap();
                cfg.CreateMap<SolidSampler, SolidSamplerDto>().ReverseMap();
                cfg.CreateMap<CheckerSampler, CheckerSamplerDto>().ReverseMap();
                cfg.CreateMap<SkySampler, SkySamplerDto>().ReverseMap();
                cfg.CreateMap<RainbowSampler, RainbowSamplerDto>().ReverseMap();
                cfg.CreateMap<Sampler, SamplerHolder>()
                    .ForMember(d => d.CheckerSampler, o => o.MapFrom(s => s as CheckerSampler))
                    .ForMember(d => d.SolidSampler, o => o.MapFrom(s => s as SolidSampler))
                    .ForMember(d => d.SkySampler, o => o.MapFrom(s => s as SkySampler))
                    .ForMember(d => d.RainbowSampler, o => o.MapFrom(s => s as RainbowSampler))
                    .ReverseMap().ConvertUsing((s, d, c) =>
                    {
                        var item = s.CheckerSampler ?? s.SolidSampler ?? s.SkySampler ?? (SamplerDto)s.RainbowSampler;
                        return c.Mapper.Map(item, d);
                    });

                cfg.CreateMap<string, SamplerHolder>().ConvertUsing((s, _, _) =>
                {
                    Match match;
                    if ((match = RgbExp.Match(s)).Success &&
                        float.TryParse(match.Groups[1].Value, out var r) &&
                        float.TryParse(match.Groups[2].Value, out var g) &&
                        float.TryParse(match.Groups[3].Value, out var b))
                        return new SamplerHolder { SolidSampler = new SolidSamplerDto { Color = new Rgb(r, g, b) } };
                    if ((match = VecExp.Match(s)).Success &&
                        float.TryParse(match.Groups[1].Value, out var x) &&
                        float.TryParse(match.Groups[2].Value, out var y) &&
                        float.TryParse(match.Groups[3].Value, out var z))
                        return new SamplerHolder { SolidSampler = new SolidSamplerDto { Color = new Vector3(x, y, z) } };
                    throw new InvalidDataException();
                });
                cfg.CreateMap<object, Sampler>().ConvertUsing((s, d, c) =>
                {
                    var item = s switch
                    {
                        Dictionary<object, object> dict => c.Mapper.Map<SamplerHolder>(ConvertDictionaries(dict)),
                        string str => c.Mapper.Map<SamplerHolder>(str),
                        _ => throw new NotSupportedException()
                    };
                    
                    return c.Mapper.Map(item, d);
                });

                cfg.CreateMap<string, Vector3>().ConvertUsing((s, d, c) =>
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

                cfg.CreateMap<Vector3, Rgb>().ConvertUsing(s => colorModel.VectorToRgb(s));
                cfg.CreateMap<Rgb, Vector3>().ConvertUsing(s => colorModel.RgbToVector(s));

                cfg.CreateMap<TransformDto, Matrix4x4>().ConvertUsing((s, _, _) => s switch
                {
                    RotateTransformDto x when x.Center.HasValue =>
                        Matrix4x4.CreateTranslation(-x.Center.Value) *
                        Matrix4x4.CreateFromYawPitchRoll(DegToRad(x.Rotation.Y), DegToRad(x.Rotation.X),
                            DegToRad(x.Rotation.Z)) *
                        Matrix4x4.CreateTranslation(x.Center.Value),
                    RotateTransformDto x => Matrix4x4.CreateRotationZ(DegToRad(x.Rotation.Z)) *
                                            Matrix4x4.CreateRotationY(DegToRad(x.Rotation.Y)) *
                                            Matrix4x4.CreateRotationX(DegToRad(x.Rotation.X)),
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
            });
#if DEBUG
            config.AssertConfigurationIsValid();
#endif
            return config.CreateMapper();
        }
        
        public static Scene ReadScene(string yamlString)
        {
            var deserializer = new DeserializerBuilder().WithTypeConverter(new Vector3Converter())
                .WithTypeConverter(new RgbConverter()).Build();
            var dto = deserializer.Deserialize<SceneDto>(yamlString);
            
            return CreateMapper(dto.ColorModel).Map<Scene>(dto);
        }

        public static string WriteScene(Scene scene)
        {
            var dto = CreateMapper(scene.ColorModel).Map<SceneDto>(scene);
            var serializer = new SerializerBuilder().WithTypeConverter(new Vector3Converter())
                .WithTypeConverter(new RgbConverter())
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
            return serializer.Serialize(dto);
        }

        private static Dictionary<string, Matrix4x4> BuildMatrixDict(IEnumerable<TransformHolder> transformHolders,
            Matrix4x4 matrix, ResolutionContext c, Dictionary<string, Matrix4x4> dict = null)
        {
            dict ??= new Dictionary<string, Matrix4x4>();

            var transformDtos = c.Mapper.Map<List<TransformDto>>(transformHolders);
            foreach (var transformDto in transformDtos)
            {
                var nextMatrix = c.Mapper.Map<Matrix4x4>(transformDto);
                var compositeMatrix = nextMatrix * matrix;
                if (!string.IsNullOrWhiteSpace(transformDto.Name))
                    dict.Add(transformDto.Name, compositeMatrix);
                BuildMatrixDict(transformDto.Children, compositeMatrix, c, dict);
            }

            return dict;
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
    }
}
