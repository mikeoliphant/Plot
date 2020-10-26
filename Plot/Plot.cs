using System.Collections.ObjectModel;

namespace Plot
{
    public class PlotFunction
    {
        string name;
        string code;

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

        public PlotFunction(string name, string code)
        {
            this.name = name;
            this.code = code;
        }
    }

    public class PlotGenerator
    {
        ObservableCollection<PlotFunction> plotFunctions = new ObservableCollection<PlotFunction>();
        PlotFunction startValueFunction = new PlotFunction("StartValue", "0");
        PlotFunction endValueFunction = new PlotFunction("EndValue", "16 * Math.PI");
        private CodeExecutor codeGenerator;
        private int functionNumber = 1;

        public ObservableCollection<PlotFunction> PlotFunctions
        {
            get { return plotFunctions; }
        }

        public PlotFunction StartValueFunction
        {
            get { return startValueFunction; }
            set { startValueFunction = value; }
        }

        public PlotFunction EndValueFunction
        {
            get { return endValueFunction; }
            set { endValueFunction = value; }
        }

        public double StartX
        {
            get { return (double)codeGenerator.CallFunction(startValueFunction.Name); }
        }

        public double EndX
        {
            get { return (double)codeGenerator.CallFunction(endValueFunction.Name); }
        }

        public PlotGenerator()
        {
            AddFunction("Math.Sin(X)");
            AddFunction("Math.Sin(X * 2) * .25");
        }

        public void AddFunction(string code)
        {
            plotFunctions.Add(new PlotFunction("Func" + functionNumber++, code));
        }

        public void Compile()
        {
            codeGenerator = new CodeExecutor();

            codeGenerator.AddProperty("X", typeof(double));

            codeGenerator.AddFunction(startValueFunction.Name, startValueFunction.Code, typeof(double));
            codeGenerator.AddFunction(endValueFunction.Name, endValueFunction.Code, typeof(double));

            foreach (PlotFunction function in plotFunctions)
            {
                codeGenerator.AddFunction(function.Name, function.Code, typeof(double));
            }

            codeGenerator.Compile();
        }

        public void SetX(double x)
        {
            codeGenerator.SetPropertyValue("X", x);
        }

        public double GetY(string functionName)
        {
            return (double)codeGenerator.CallFunction(functionName);
        }
    }
}
