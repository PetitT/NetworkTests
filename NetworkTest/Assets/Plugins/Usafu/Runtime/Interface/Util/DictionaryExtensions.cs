using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FishingCactus.Util
{
    public static class DictionaryExtensions
    {
        public static bool TryGetKey< TValue, TKey >(
            this Dictionary< TKey, TValue > dictionary,
            TValue value,
            out TKey key
            )
        {
            var c = EqualityComparer< TValue >.Default;
            foreach( var element in dictionary )
            {
                if( !c.Equals( element.Value, value ) )
                {
                    continue;
                }

                key = element.Key;
                return true;
            }

            key = default( TKey );

            return false;
        }
    }
}