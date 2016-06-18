using DynamicProxy.Fody;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.IO;
using DynamicProxy.Emit;

namespace DynamicProxy.CecilEmit
{
    public class CecilProxyTypeFactory : EmitProxyTypeFactory, IProxyTypeFactory
    {
        public override Type CreateProxyType(Type sourceType)
        {
            Type targetType;
            var proxiedName = $"{sourceType.Namespace}.{sourceType.Name.Replace("`", "$")}$Proxy";
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Contains(proxiedName)))
            {
                targetType = asm.GetTypes().FirstOrDefault(x => x.Name.Equals(sourceType.Name));
                if (targetType != null)
                {
                    return targetType;
                }
            }

            targetType = base.CreateProxyType(sourceType);

            var dynamic = new DynamicProxyWeaver();
            var context = dynamic.Context;

            BuildIt(context.ModuleDefinition.FindType(sourceType.Namespace, sourceType.Name).Resolve(), proxiedName, dynamic, context);

            return targetType;
        }

        private static void BuildIt(TypeDefinition sourceType, string proxiedName, DynamicProxyWeaver dynamic, WeaverContext context)
        {
            context.ModuleDefinition = ModuleDefinition.CreateModule(proxiedName, ModuleKind.Dll);
            ClassWeaver classWeaver;
            if (sourceType.IsInterface)
                classWeaver = new InterfaceClassWeaver(context, sourceType);
            else if (dynamic.ProxyInterface.Resolve().IsAssignableFrom(sourceType))
                classWeaver = new InPlaceClassWeaver(context, sourceType);
            else if (dynamic.ReverseProxyInterface.Resolve().IsAssignableFrom(sourceType))
                classWeaver = new ReverseProxyClassWeaver(context, sourceType);
            else
                classWeaver = new NonInterfaceClassWeaver(context, sourceType);
        }
    }
}
