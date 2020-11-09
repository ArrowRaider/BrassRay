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
        // This mess returns a mapping between concrete-space and dto-space
        private static readonly Lazy<IMapper> Mapper = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Scene, SceneDto>()
                    .ForMember(d => d.Materials, o => o.MapFrom((s, _) => s.Drawables.Select(x => x.Material).Distinct()))
                    .ReverseMap()
                    .AfterMap((s, d, c) =>
                    {
                        var materials = c.Mapper.Map<List<Material>>(s.Materials);
                        var drawableDtos = c.Mapper.Map<List<DrawableDto>>(s.Drawables);
                        var materialDict = materials.ToDictionary(m => m.Name);
                        foreach (var (sd, dd) in drawableDtos.Zip(d.Drawables, (sd, dd) => (sd, dd)))
                        {
                            dd.Material = materialDict[sd.Material];
                        }
                    });

                cfg.CreateMap<Background, BackgroundDto>()
                    .Include<SolidBackground, SolidBackgroundDto>()
                    .Include<SkyBackground, SkyBackgroundDto>().ReverseMap();
                cfg.CreateMap<SolidBackground, SolidBackgroundDto>().ReverseMap();
                cfg.CreateMap<SkyBackground, SkyBackgroundDto>().ReverseMap();
                cfg.CreateMap<Background, BackgroundHolder>()
                    .ForMember(d => d.SkyBackground, o => o.MapFrom(s => s as SkyBackground))
                    .ForMember(d => d.SolidBackground, o => o.MapFrom(s => s as SolidBackground))
                    .ReverseMap().ConvertUsing((s, d, c) =>
                    {
                        var item = s.SkyBackground ?? (BackgroundDto)s.SolidBackground;
                        return c.Mapper.Map(item, d);
                    });

                cfg.CreateMap<Drawable, DrawableDto>()
                    .ForMember(d => d.Material, o => o.MapFrom(s => s.Material.Name))
                    .Include<InfinitePlane, InfinitePlaneDto>()
                    .Include<Box, BoxDto>()
                    .Include<Sphere, SphereDto>()
                    .ReverseMap().ForMember(d => d.Material, o => o.Ignore());
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
    }
}
