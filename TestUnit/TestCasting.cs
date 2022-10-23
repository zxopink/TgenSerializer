namespace TestUnit
{
    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [TestCase(5)]
        public void TestInt(int number)
        {
            byte[] bytes = Bytes.P2B(number);
            int after = Bytes.B2P<int>(bytes);
            Assert.That(after, Is.EqualTo(number));
        }
    }
}