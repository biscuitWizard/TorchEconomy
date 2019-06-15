using System;
using System.Linq;
using System.Reflection;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using Torch.API.Managers;
using TorchEconomy.Data;
using TorchEconomy.Managers;

namespace TorchEconomy
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
                if (EconomyPlugin.Instance.Config.MySQL)
                {
                    _.For<IConnectionFactory>()
                        .Use<MysqlConnectionFactory>();
                }
                else if(EconomyPlugin.Instance.Config.SqlLite)
                {
                    _.For<IConnectionFactory>()
                        .Use<SqliteConnectionFactory>();
                }

                _.Scan(s =>
                {
                    // Assembly to scan.
                    var assemblies = AppDomain
                        .CurrentDomain
                        .GetAssemblies()
                        .Where(a => a.GetName().Name.StartsWith("TorchEconomy.") 
                                    || a.GetName().Name == "TorchEconomy")
                        .ToList();
                    foreach (var assembly in assemblies)
                        s.Assembly(assembly);

                    // All managers should be singletons.
                    s.Convention<SingletonConvention<BaseManager>>();
                });

                _.For<IMultiplayerManagerServer>()
                    .Use((context) => EconomyPlugin
                        .Instance
                        .Torch
                        .CurrentSession
                        .Managers
                        .GetManager(typeof(IMultiplayerManagerServer)) as IMultiplayerManagerServer);

                _.For<DefinitionResolver>()
                    .Use(new DefinitionResolver())
                    .Singleton();
            });
        }
    }
}