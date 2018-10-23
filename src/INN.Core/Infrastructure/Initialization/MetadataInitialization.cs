using System;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Meta.Management;
using Mediachase.Commerce.Customers;

namespace INN.Core.Infrastructure.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    internal class MetadataInitialization : IInitializableModule
    {
        protected static readonly ILogger Logger = LogManager.GetLogger(typeof(MetadataInitialization));

        public void Initialize(InitializationEngine context)
        {
            CreateMetaField(AddressEntity.ClassName, Constants.AddressIdentifierFieldName, Constants.AddressIdentifierFieldName);
        }

        private void CreateMetaField(string metaClassName, string metaFieldName, string friendlyName, bool isNullable = true, int maxLength = 255, bool isUnique = false)
        {
            var metaClass = GetMetaClass(metaClassName, metaFieldName);
            if (metaClass == null)
            {
                return;
            }

            using(var metaFieldBuilder = new MetaFieldBuilder(metaClass))
            {
                metaFieldBuilder.MetaClass.AccessLevel = AccessLevel.Customization;
                metaFieldBuilder.CreateText(metaFieldName, friendlyName, isNullable, maxLength, isUnique);
                metaFieldBuilder.SaveChanges();
            }
        }

        private static MetaClass GetMetaClass(string metaClassName, string metaFieldName)
        {
            var metaClass = DataContext.Current.GetMetaClass(metaClassName);
            if (metaClass == null || metaClass.Fields[metaFieldName] != null)
            {
                return null;
            }

            return metaClass;
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
