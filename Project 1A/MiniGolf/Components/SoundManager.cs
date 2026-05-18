using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Linq;

namespace MiniGolf.Components
{
    public class SoundManager : DrawableGameComponent
    {
        private SoundEffect _swingSound;
        private SoundEffect _bounceSound;
        private SoundEffect _holeSound;

        private BallComponent _ball;
        private PhysicsEngine _physics;
        private InputComponent _input;

        private bool _wasMoving = false;
        private bool _holeSoundPlayed = false;


        private double _bounceCooldown = 0;
        private const double BounceCooldownTime = 0.2;
        public SoundManager(Game game) : base(game) { }

        public override void Initialize()
        {
            _ball = Game.Components.OfType<BallComponent>().FirstOrDefault();
            _physics = Game.Components.OfType<PhysicsEngine>().FirstOrDefault();
            _input = Game.Components.OfType<InputComponent>().FirstOrDefault();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            ContentManager content = Game.Content;
            _swingSound = content.Load<SoundEffect>("swing");
            _bounceSound = content.Load<SoundEffect>("bounce");
            _holeSound = content.Load<SoundEffect>("hole");
        }

        public override void Update(GameTime gameTime)
{
    if (_bounceCooldown > 0)
        _bounceCooldown -= gameTime.ElapsedGameTime.TotalSeconds;

    // Play swing sound when shot is fired
    if (_input.ShotFired == false && !_wasMoving && _ball.IsMoving)
        _swingSound?.Play();

    // Play hole sound once when ball sinks
    if (_physics.BallInHole && !_holeSoundPlayed)
    {
        _holeSound?.Play();
        _holeSoundPlayed = true;
    }

    _wasMoving = _ball.IsMoving;
}
        public void PlayBounce()
{
    if (_bounceCooldown <= 0)
    {
        _bounceSound?.Play();
        _bounceCooldown = BounceCooldownTime;
    }
}
    }
}