using System.Linq;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using Torch.API.Managers;
using TorchEconomySE.Data;
using TorchEconomySE.Managers;

namespace TorchEconomySE
{
    internal class SingletonConvention<TPluginFamily> : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, Registry registry)
        {
            foreach (var type in types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed)
                .Where(type => typeof(TPluginFamily).IsAssignableFrom(type)))
            {
                registry.ForSingletonOf(type);
                registry.For(typeof(TPluginFamily)).Use(c => c.GetInstance(type));
                //registry.For(type).Use(c => c.GetInstance(type));
            }
        }
    }
    
    internal static class IoC
    {
        public static void Initialize(this IContainer container)
        {
            container.Configure(_ =>
            {
                _.For<IConnectionFactory>()
                    .Use<MysqlConnectionFactory>();
                
                _.Scan(s =>
                {
                    // Assembly to scan.
                    s.AssemblyContainingType(typeof(BaseManager));

                    // Registration conventions.
                    s.AddAllTypesOf<BaseManager>();
                    s.WithDefaultConventions();
                });

                _.For<IMultiplayerManagerServer>()
                    .Use((context) => EconomyPlugin
                        .Instance
                        .Torch
                        .CurrentSession
                        .Managers
                        .GetManager<IMultiplayerManagerServer>());
            });
        }
    }
}