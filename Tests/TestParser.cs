using NUnit.Framework;
using Unity.GoQL;

namespace Tests
{
    public class TestParser
    {
        
        [Test]
        public void TestParserSimple()
        {
            var tokenizer = new Tokenizer(); 
            //select root gameobject named "gameObjectName"
            var code = Parser.Parse("/gameObjectName");
            Assert.AreEqual(GoQLCode.EnterChildren, code[0]);
            Assert.AreEqual("gameObjectName", code[1]);
        }


    }
}
