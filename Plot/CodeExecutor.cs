using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;

namespace Plot
{
    public class CodeFunction
    {
        private string name;
        private string code;
        private Type returnType;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public Type ReturnType
        {
            get { return returnType; }
            set { returnType = value; }
        }

        public CodeFunction(string name, string code, Type returnType)
        {
            this.name = name;
            this.code = code;
            this.returnType = returnType;
        }
    }

    public class CodeProperty
    {
        private string name;
        private Type type;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Type Type
        {
            get { return type; }
            set { type = value; }
        }

        public CodeProperty(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }
    }

    public class CodeExecutor
    {
        CodeDomProvider codeProvider;
        CompilerParameters compilerParameters;
        Assembly codeAssembly;
        Dictionary<string, CodeFunction> codeFunctions = new Dictionary<string, CodeFunction>();
        Dictionary<string, CodeProperty> codeProperties = new Dictionary<string, CodeProperty>();

        public CodeExecutor()
        {
            codeProvider = new CSharpCodeProvider();
            compilerParameters = new CompilerParameters();

            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = true;
            compilerParameters.IncludeDebugInformation = false;
            compilerParameters.WarningLevel = 3;
            compilerParameters.TreatWarningsAsErrors = false;
            compilerParameters.CompilerOptions = "/optimize";

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            compilerParameters.ReferencedAssemblies.Add(executingAssembly.Location);

            foreach (AssemblyName assemblyName in executingAssembly.GetReferencedAssemblies())
            {
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(assemblyName).Location);
            }
        }

        public void AddFunction(string name, string code, Type returnType)
        {
            if (returnType != null)
            {
                code = code.Trim();

                if (!code.EndsWith(";"))
                    code = "return " + code + ";";
            }

            codeFunctions.Add(name, new CodeFunction(name, code, returnType));
        }

        public void AddProperty(string name, Type type)
        {
            codeProperties.Add(name, new CodeProperty(name, type));
        }

        public void Compile()
        {
            string assemblyCode = "using System; public class Evaluator {";

            foreach (CodeProperty property in codeProperties.Values)
            {
                assemblyCode += "private static " + property.Type.ToString() + " _" + property.Name + "; ";
            }

            foreach (CodeProperty property in codeProperties.Values)
            {
                assemblyCode += "public static " + property.Type.ToString() + " " + property.Name + " { " +
                    " get { return " + " _" + property.Name + "; } " +
                    " set { _" + property.Name + " = value; } " +
                    "}";
            }

            foreach (CodeFunction function in codeFunctions.Values)
            {
                if (function.ReturnType != null)
                {
                    assemblyCode += "public static " + function.ReturnType.ToString() + " " +
                        function.Name + "() { " + function.Code + " }";
                }
                else
                {
                    assemblyCode += "public static void " + function.Name + "() { " + function.Code + "; }";
                }
            }

            assemblyCode += "}";

            codeAssembly = CompileAssembly(assemblyCode);
        }

        public object CallFunction(string functionName)
        {
            CodeFunction function = codeFunctions[functionName];

            Type t = codeAssembly.GetType("Evaluator");
            MethodInfo evaluateMethod = t.GetMethod(function.Name);

            if (function.ReturnType != null)
            {
                return evaluateMethod.Invoke(null, null);
            }
            else
            {
                evaluateMethod.Invoke(null, null);
            }

            return null;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            CodeProperty property = codeProperties[propertyName];

            Type t = codeAssembly.GetType("Evaluator");
            PropertyInfo propInfo = t.GetProperty(property.Name);

            propInfo.SetValue(t, value, null);
        }

        private Assembly CompileAssembly(string assemblyCode)
        {
            CompilerResults compileResults =
                codeProvider.CompileAssemblyFromSource(compilerParameters, new string[] { assemblyCode });

            if (compileResults.Errors.Count > 0)
            {
                string errorStr = "";

                foreach (CompilerError error in compileResults.Errors)
                {
                    errorStr += error.ErrorNumber.ToString() + ": " + error.ErrorText + "\n";
                }

                throw new Exception(errorStr);
            }
            else
            {
                return compileResults.CompiledAssembly;
            }
        }
    }
}
