using NUnit.Framework;

namespace MMS.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Pipeline;

    [TestFixture]
    public class NewtonsoftJsonMessageSerializerTest
    {
        [Test]
        public void UsesProvidedSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new MessageConverter() }
            };
            
            var testee = new NewtonsoftJsonMessageSerializer(() => settings);

            var message = Message.Create("Text");
            
            using (var target = new MemoryStream())
            {
                testee.Serialize(message, target);
                var result = (Message) testee.Deserialize(target, typeof(Message));

                result.ShouldBeEquivalentTo(message);
            }
        }

        [Test]
        public void UsesDefaultSerializerSettings_WhenNoSettingsProvided()
        {
            var testee = new NewtonsoftJsonMessageSerializer();
            
            var messageNotSerializableByDefaultConverters = Message.Create("Text");
            
            using (var target = new MemoryStream())
            {
                testee.Serialize(messageNotSerializableByDefaultConverters, target);
                // ReSharper disable once AccessToDisposedClosure delegate is executed inside the using clause
                Action act = () => testee.Deserialize(target, typeof(Message));
                
                AssertThatDefaultSerializerSettingsAreUsed(act);
            }
        }

        private static void AssertThatDefaultSerializerSettingsAreUsed(Action act)
        {
            act.ShouldThrow<JsonSerializationException>()
                .Which.Message.Should().Contain(typeof(Message).FullName);
        }

        private class Message
        {
            private Message(string text)
            {
                this.Text = text;
            }

            public string Text { get; }

            public static Message Create(string text)
            {
                return new Message(text);
            }
        }

        private class MessageConverter : JsonConverter
        {
            private static readonly Type ConvertableType = typeof(Message);
            
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var message = (Message)value;
                writer.WriteValue(message.Text);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var value = (string)reader.Value;
                return Message.Create(value);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == ConvertableType;
            }
        }
    }
}