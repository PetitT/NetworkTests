namespace FishingCactus.User
{
    public class UniqueUserId : IUniqueUserId
    {
        private static readonly int _invalidUserId = -1;

        public int UniqueId { get; }

        public bool IsValid => UniqueId != _invalidUserId;

        public UniqueUserId()
        {
            UniqueId = _invalidUserId;
        }

        public UniqueUserId( int value )
        {
            UniqueId = value;
        }

        public override string ToString()
        {
            return UniqueId.ToString();
        }

        public override bool Equals( object obj )
        {
            return Equals( ( UniqueUserId ) obj );
        }

        public override int GetHashCode()
        {
            return UniqueId.GetHashCode();
        }

        public bool Equals( IUniqueUserId other )
        {
            if ( !( other is UniqueUserId other_user_id ) )
            {
                return false;
            }

            return other_user_id.UniqueId == UniqueId;
        }
    }
}