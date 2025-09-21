using Xunit;

namespace JO2024API.Tests
{
    public class TestsBasiques
    {
        [Fact]
        public void Test_01_Compilation()
        {
            // Vérifier que les tests peuvent s'exécuter
            Assert.True(true);
        }

        [Fact] 
        public void Test_02_Mathematiques()
        {
            // Test simple
            var result = 2 + 2;
            Assert.Equal(4, result);
        }
    }
}