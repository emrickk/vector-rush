using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class ArcadeHudTests
    {
        [Test]
        public void SairaFacesAreBundledAndHaveHudGlyphs()
        {
            foreach(var name in new[]{"Saira-BlackItalic","Saira-MediumItalic"})
            {
                var font=UnityEngine.Resources.Load<UnityEngine.Font>("Fonts/"+name);
                Assert.That(font,Is.Not.Null,name);
                foreach(char c in "0123456789/:KMHLAP") Assert.That(font.HasCharacter(c),Is.True,name+" "+c);
            }
        }
        [Test]
        public void BoostIntensityRisesWithSpeedAndIsGated()
        {
            Assert.That(RaceHUD.ArcadeBoostTarget(175,true,false,true),Is.EqualTo(.25f));
            Assert.That(RaceHUD.ArcadeBoostTarget(260,true,false,true),Is.EqualTo(1f));
            Assert.That(RaceHUD.ArcadeBoostTarget(900,true,false,true),Is.EqualTo(1f));
            Assert.That(RaceHUD.ArcadeBoostTarget(260,false,false,true),Is.Zero);
            Assert.That(RaceHUD.ArcadeBoostTarget(260,true,true,true),Is.Zero);
            Assert.That(RaceHUD.ArcadeBoostTarget(260,true,false,false),Is.Zero);
        }
        [Test]
        public void BoostSettlesAndAccessibilitySuppressesImmediately()
        {
            float amount=RaceHUD.ArcadeBoostStep(0,1,.1f,false);
            Assert.That(amount,Is.InRange(.5f,.8f));
            Assert.That(RaceHUD.ArcadeBoostStep(amount,0,1,false),Is.LessThan(.003f));
            Assert.That(RaceHUD.ArcadeBoostStep(1,1,0,true),Is.Zero);
            Assert.That(RaceHUD.ArcadeBoostStep(.5f,1,0,false),Is.EqualTo(.5f));
            float one=RaceHUD.ArcadeBoostStep(0,1,.1f,false),split=0;
            for(int i=0;i<10;i++)split=RaceHUD.ArcadeBoostStep(split,1,.01f,false);
            Assert.That(split,Is.EqualTo(one).Within(.00001f));
        }
        [Test]
        public void VibrationIsBoundedAndZeroAtRest()
        {
            for(int i=0;i<1000;i++)
            {
                var p=RaceHUD.ArcadeBoostOffset(1,i*.013f);
                Assert.That(UnityEngine.Mathf.Abs(p.x),Is.LessThanOrEqualTo(2.2f));
                Assert.That(UnityEngine.Mathf.Abs(p.y),Is.LessThanOrEqualTo(1.1f));
                Assert.That(RaceHUD.ArcadeBoostOffset(0,i),Is.EqualTo(UnityEngine.Vector2.zero));
            }
        }
        [TestCase(-1f, 0f)]
        [TestCase(0f, 0f)]
        [TestCase(.1f, .5f)]
        [TestCase(.2f, 1f)]
        [TestCase(.5f, 2.5f)]
        [TestCase(1f, 5f)]
        [TestCase(2f, 5f)]
        public void FiveCellsRepresentClampedLiveCharge(float charge, float total)
        {
            float sum = 0;
            for (int i = 0; i < 5; i++)
            {
                float fill = RaceHUD.ArcadeCellFill(charge, i);
                Assert.That(fill, Is.InRange(0f, 1f));
                sum += fill;
            }
            Assert.That(sum, Is.EqualTo(total).Within(.0001f));
        }

        [Test]
        public void ChargeFillsCellsFromLeftToRight()
        {
            Assert.That(RaceHUD.ArcadeCellFill(.5f, 0), Is.EqualTo(1));
            Assert.That(RaceHUD.ArcadeCellFill(.5f, 1), Is.EqualTo(1));
            Assert.That(RaceHUD.ArcadeCellFill(.5f, 2), Is.EqualTo(.5f));
            Assert.That(RaceHUD.ArcadeCellFill(.5f, 3), Is.Zero);
            Assert.That(RaceHUD.ArcadeCellFill(.5f, 4), Is.Zero);
        }
    }
}
