using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Formatting.Json;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Serilog.Formatting.Json.JsonFormatter json=new JsonFormatter();
        }
    }
}
