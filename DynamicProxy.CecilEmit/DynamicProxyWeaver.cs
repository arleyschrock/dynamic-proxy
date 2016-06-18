using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace DynamicProxy.Fody
{
    public class DynamicProxyWeaver
    {
        public DynamicProxyWeaver()
        {
            _Context = new Lazy<WeaverContext>(() => CreateContext());
        }
        public ModuleDefinition ModuleDefinition { get; set; }

        // Will log an MessageImportance.High message to MSBuild. OPTIONAL
        public Action<string> LogInfo { get; set; }

        // Will log an error message to MSBuild. OPTIONAL
        public Action<string> LogError { get; set; }

        public Action<string> LogWarning { get; set; }

        public void Execute()
        {

            TypeDefinition[] targetTypes;

            var proxyAttribute = ModuleDefinition.FindType("DynamicProxy", "ProxyAttribute", DynamicProxy);
            if (proxyAttribute == null)
                throw new Exception($"{nameof(proxyAttribute)} is null");
            var proxyForAttribute = ModuleDefinition.FindType("DynamicProxy", "ProxyForAttribute", DynamicProxy);
            var doNotProxyAttribute = ModuleDefinition.FindType("DynamicProxy", "DoNotProxyAttribute", DynamicProxy);
            var originalMethodAttributeConstructor = ModuleDefinition.FindConstructor(ModuleDefinition.FindType("DynamicProxy", "OriginalMethodAttribute", DynamicProxy));

            targetTypes = ModuleDefinition.GetAllTypes().Where(x => x.IsDefined(proxyAttribute, true) || ReverseProxyInterface.IsAssignableFrom(x) || ProxyInterface.IsAssignableFrom(x)).ToArray();

            // Get external proxy references
            var proxyFors = ModuleDefinition.Assembly.GetCustomAttributes(proxyForAttribute).Select(x => (TypeReference)x.ConstructorArguments.Single().Value).Select(x => x.Resolve()).ToArray();
            targetTypes = targetTypes.Concat(proxyFors).ToArray();
            foreach (var sourceType in targetTypes)
            {
                LogInfo($"Emitting proxy for {sourceType.FullName}");
                ClassWeaver classWeaver;

                if (sourceType.IsInterface)
                    classWeaver = new InterfaceClassWeaver(Context, sourceType);
                else if (ProxyInterface.IsAssignableFrom(sourceType))
                    classWeaver = new InPlaceClassWeaver(Context, sourceType);
                else if (ReverseProxyInterface.IsAssignableFrom(sourceType))
                    classWeaver = new ReverseProxyClassWeaver(Context, sourceType);
                else
                    classWeaver = new NonInterfaceClassWeaver(Context, sourceType);

                classWeaver.Execute();
            }
        }
        
        public TypeReference ReverseProxyInterface => ModuleDefinition.FindType("DynamicProxy", "IReverseProxy", DynamicProxy);

        public TypeReference ProxyInterface => ModuleDefinition.FindType("DynamicProxy", "IProxy", DynamicProxy);

        public WeaverContext Context => _Context.Value;
        private Lazy<WeaverContext> _Context;
        public AssemblyNameReference DynamicProxy => ModuleDefinition.FindAssembly("DynamicProxy");

        private WeaverContext CreateContext()
        {
            var proxyForAttribute = ModuleDefinition.FindType("DynamicProxy", "ProxyForAttribute", DynamicProxy);
            var doNotProxyAttribute = ModuleDefinition.FindType("DynamicProxy", "DoNotProxyAttribute", DynamicProxy);
            var originalMethodAttributeConstructor = ModuleDefinition.FindConstructor(ModuleDefinition.FindType("DynamicProxy", "OriginalMethodAttribute", DynamicProxy));
            
            var methodInfoType = ModuleDefinition.Import(typeof(MethodInfo));
            var propertyInfoType = ModuleDefinition.Import(typeof(PropertyInfo));

            var func2Type = ModuleDefinition.Import(typeof(Func<,>));
            var action1Type = ModuleDefinition.Import(typeof(Action<>));
            var objectArrayType = ModuleDefinition.Import(typeof(object[]));
            var taskType = ModuleDefinition.Import(typeof(Task));
            var invocationTType = ModuleDefinition.FindType("DynamicProxy", "InvocationT`1", DynamicProxy, "T");
            var asyncInvocationTType = ModuleDefinition.FindType("DynamicProxy", "AsyncInvocationT`1", DynamicProxy, "T");
            var invocationHandlerType = ModuleDefinition.FindType("DynamicProxy", "InvocationHandler", DynamicProxy);
            var invocationHandlerIsHandlerActive = ModuleDefinition.FindMethod(invocationHandlerType, "IsHandlerActive");
            var voidInvocationType = ModuleDefinition.FindType("DynamicProxy", "VoidInvocation", DynamicProxy);
            var voidInvocationConstructor = ModuleDefinition.FindConstructor(voidInvocationType);
            var voidAsyncInvocationType = ModuleDefinition.FindType("DynamicProxy", "VoidAsyncInvocation", DynamicProxy);
            var voidAsyncInvocationConstructor = ModuleDefinition.FindConstructor(voidAsyncInvocationType);
            var voidInvokeMethod = ModuleDefinition.FindMethod(invocationHandlerType, "VoidInvoke");
            var asyncVoidInvokeMethod = ModuleDefinition.FindMethod(invocationHandlerType, "VoidAsyncInvoke");
            var invokeTMethod = ModuleDefinition.FindMethod(invocationHandlerType, "InvokeT");
            var asyncInvokeTMethod = ModuleDefinition.FindMethod(invocationHandlerType, "AsyncInvokeT");
            var objectType = ModuleDefinition.Import(typeof(object));
            var proxyGetInvocationHandlerMethod = ModuleDefinition.FindGetter(ProxyInterface, "InvocationHandler");
            var reverseProxyGetInvocationHandlerMethod = ModuleDefinition.FindGetter(ReverseProxyInterface, "InvocationHandler");
            var invocationType = ModuleDefinition.FindType("DynamicProxy", "Invocation", DynamicProxy);
            var invocationGetArguments = ModuleDefinition.FindGetter(invocationType, "Arguments");
            var invocationGetProxy = ModuleDefinition.FindGetter(invocationType, "Proxy");
            var asyncTaskMethodBuilder = ModuleDefinition.Import(typeof(AsyncTaskMethodBuilder<>));
            var methodFinder = ModuleDefinition.FindType("DynamicProxy.Reflection", "MethodFinder`1", DynamicProxy, "T");
            var findMethod = ModuleDefinition.FindMethod(methodFinder, "FindMethod");
            var findProperty = ModuleDefinition.FindMethod(methodFinder, "FindProperty");

            return new WeaverContext
            {
                ModuleDefinition = ModuleDefinition,
                LogWarning = LogWarning,
                LogError = LogError,
                LogInfo = LogInfo,
                DynamicProxy = DynamicProxy,
                MethodInfoType = methodInfoType,
                PropertyInfoType = propertyInfoType,
                Action1Type = action1Type,
                AsyncInvocationTType = asyncInvocationTType,
                Func2Type = func2Type,
                InvocationTType = invocationTType,
                ObjectArrayType = objectArrayType,
                TaskType = taskType,
                AsyncInvokeTMethod = asyncInvokeTMethod,
                AsyncVoidInvokeMethod = asyncVoidInvokeMethod,
                InvocationHandlerType = invocationHandlerType,
                InvocationHandlerIsHandlerActive = invocationHandlerIsHandlerActive,
                InvokeTMethod = invokeTMethod,
                ObjectType = objectType,
                VoidAsyncInvocationConstructor = voidAsyncInvocationConstructor,
                VoidInvocationConstructor = voidInvocationConstructor,
                VoidInvokeMethod = voidInvokeMethod,
                ProxyGetInvocationHandlerMethod = proxyGetInvocationHandlerMethod,
                ReverseProxyGetInvocationHandlerMethod = reverseProxyGetInvocationHandlerMethod,
                InvocationType = invocationType,
                VoidInvocationType = voidInvocationType,
                VoidAsyncInvocationType = voidAsyncInvocationType,
                InvocationGetArguments = invocationGetArguments,
                InvocationGetProxy = invocationGetProxy,
                AsyncTaskMethodBuilder = asyncTaskMethodBuilder,
                MethodFinder = methodFinder,
                FindMethod = findMethod,
                FindProperty = findProperty,
                DoNotProxyAttribute = doNotProxyAttribute,
                OriginalMethodAttributeConstructor = originalMethodAttributeConstructor
            };


        }
    }
}