using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.GoQL;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TestTokens
    {
        
        [Test]
        public void TestTokenSimple()
        {
            var tokenizer = new Tokenizer(); 
            //select root gameobject named "gameObjectName"
            var tokens = tokenizer.Tokenize("/gameObjectName");
            Assert.AreEqual(tokens.Count, 2);
            Assert.AreEqual(tokens[0].type, TokenType.Slash);
            Assert.AreEqual(tokens[1].type, TokenType.String);
        }

        [Test]
        public void TestTokenIntermediate()
        {
            var tokenizer = new Tokenizer();
            //select specified range of children of childName from parent gameObjectName
            var tokens = tokenizer.Tokenize("/gameObjectName/childName[1:-1]");
            Assert.AreEqual(tokens.Count, 9);
            Assert.AreEqual(tokens[0].type, TokenType.Slash);
            Assert.AreEqual(tokens[1].type, TokenType.String);
            Assert.AreEqual(tokens[2].type, TokenType.Slash);
            Assert.AreEqual(tokens[3].type, TokenType.String);
            Assert.AreEqual(tokens[4].type, TokenType.OpenSquare);
            Assert.AreEqual(tokens[5].value, "1");
            Assert.AreEqual(tokens[5].type, TokenType.Number);
            Assert.AreEqual(tokens[6].type, TokenType.Colon);
            Assert.AreEqual(tokens[7].type, TokenType.Number);
            Assert.AreEqual(tokens[7].value, "-1");
            Assert.AreEqual(tokens[8].type, TokenType.CloseSquare);
        }

         [Test]
        public void TestTokenComplex()
        {
            var tokenizer = new Tokenizer();
            //for all root objects that start with "gameObject" and have component <component>, select child named childName.
            var tokens = tokenizer.Tokenize("/gameObject*[<component>]/childName");
            Assert.AreEqual(9, tokens.Count);
            Assert.AreEqual(tokens[0].type, TokenType.Slash);
            Assert.AreEqual(tokens[1].type, TokenType.String);
            Assert.AreEqual(tokens[2].type, TokenType.OpenSquare);
            Assert.AreEqual(tokens[3].type, TokenType.OpenAngle);
            Assert.AreEqual(tokens[4].type, TokenType.String);
            Assert.AreEqual(tokens[5].type, TokenType.CloseAngle);
            Assert.AreEqual(tokens[6].type, TokenType.CloseSquare);
            Assert.AreEqual(tokens[7].type, TokenType.Slash);
            Assert.AreEqual(tokens[8].type, TokenType.String);
        }

    }
}
