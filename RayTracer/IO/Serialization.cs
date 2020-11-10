using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using AutoMapper;
using YamlDotNet.Serialization;

namespace BrassRay.RayTracer.IO
{
    public static class Serialization
    {
        private static float DegToRad(float x) => x * MathF.PI / 180.0f;

        // This mess returns a mapping between concrete-space and dto-space
        private static readonly Lazy<IMapper> Mapper = new Lazy<IMapper>(() =>
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

                cfg.CreateMap<Environment, EnvironmentDto>()
                    .Include<SolidEnvironment, SolidEnvironmentDto>()
                    .Include<SkyEnvironment, SkyEnvironmentDto>().ReverseMap();
                cfg.CreateMap<SolidEnvironment, SolidEnvironmentDto>().ReverseMap();
                cfg.CreateMap<SkyEnvironment, SkyEnvironmentDto>().ReverseMap();
                cfg.CreateMap<Environment, EnvironmentHolder>()
                    .ForMember(d => d.SkyEnvironment, o => o.MapFrom(s => s as SkyEnvironment))
                    .ForMember(d => d.SolidEnvironment, o => o.MapFrom(s => s as SolidEnvironment))
                    .ReverseMap().ConvertUsing((s, d, c) =>
                    {
                        var item = s.SkyEnvironment ?? (EnvironmentDto)s.SolidEnvironment;
                        return c.Mapper.Map(item, d);
                    });

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
                    .Include<OrthographicCamera, OrthographicCameraDto>().ReverseMap();
                cfg.CreateMap<TargetCamera, TargetCameraDto>().ReverseMap();
                cfg.CreateMap<OrthographicCamera, OrthographicCameraDto>().ReverseMap();
                cfg.CreateMap<Camera, CameraHolder>()
                    .ForMember(d => d.TargetCamera, o => o.MapFrom(s => s as TargetCamera))
                    .ForMember(d => d.OrthographicCamera, o => o.MapFrom(s => s as OrthographicCamera))
                    .ReverseMap().ConvertUsing((s, d, c) =>
                    {
                        var item = s.TargetCamera ?? (CameraDto)s.OrthographicCamera;
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

                cfg.CreateMap<Vector3, Rgb>().ConvertUsing(s => (Rgb)s);
                cfg.CreateMap<Vector3, ClampedRgb>().ConvertUsing(s => (ClampedRgb)s);
                cfg.CreateMap<Rgb, Vector3>().ConvertUsing(s => (Vector3)s);
                cfg.CreateMap<ClampedRgb, Vector3>().ConvertUsing(s => (Vector3)s);
            });
#if DEBUG
            config.AssertConfigurationIsValid();
#endif
            return config.CreateMapper();
        });
        
        public static Scene ReadScene(string yamlString)
        {
            var deserializer = new DeserializerBuilder().WithTypeConverter(new Vector3Converter())
                .WithTypeConverter(new RgbConverter()).Build();
            var dto = deserializer.Deserialize<SceneDto>(yamlString);
            return Mapper.Value.Map<Scene>(dto);
        }

        public static string WriteScene(Scene scene)
        {
            var dto = Mapper.Value.Map<SceneDto>(scene);
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
                var nextMatrix = transformDto switch
                {
                    RotateTransformDto x when x.Center.HasValue =>
                        Matrix4x4.CreateRotationZ(DegToRad(x.Rotation.Z), x.Center.Value) *
                        Matrix4x4.CreateRotationY(DegToRad(x.Rotation.Y), x.Center.Value) *
                        Matrix4x4.CreateRotationX(DegToRad(x.Rotation.X), x.Center.Value),
                    RotateTransformDto x => Matrix4x4.CreateRotationZ(DegToRad(x.Rotation.Z)) *
                                            Matrix4x4.CreateRotationY(DegToRad(x.Rotation.Y)) *
                                            Matrix4x4.CreateRotationX(DegToRad(x.Rotation.X)),
                    ScaleTransformDto x when x.Center.HasValue => Matrix4x4.CreateScale(x.Scale, x.Center.Value),
                    ScaleTransformDto x => Matrix4x4.CreateScale(x.Scale),
                    TranslateTransformDto x => Matrix4x4.CreateTranslation(x.Offset),
                    QuaternionTransformDto x when x.Center.HasValue => 
                        Matrix4x4.CreateTranslation(x.Center.Value) *
                        Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(x.Axis, DegToRad(x.Angle))) *
                        Matrix4x4.CreateTranslation(-x.Center.Value),
                    QuaternionTransformDto x =>
                        Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(x.Axis, DegToRad(x.Angle))),
                    MatrixTransformDto x => new Matrix4x4(x.M11, x.M12, x.M13, x.M14, x.M21, x.M22, x.M23, x.M24, x.M31,
                        x.M32, x.M33, x.M34, x.M41, x.M42, x.M43, x.M44),
                    _ => throw new InvalidOperationException()
                };
                var compositeMatrix = nextMatrix * matrix;
                if (!string.IsNullOrWhiteSpace(transformDto.Name))
                    dict.Add(transformDto.Name, compositeMatrix);
                BuildMatrixDict(transformDto.Children, compositeMatrix, c, dict);
            }

            return dict;
        }
    }
}
