using System;
using System.Diagnostics;

namespace FishingCactus.Util
{
    public static class StringExtensions
    {
        public static bool MatchWildcard(
            this string str,
            string wildcard,
            StringComparison comparison_method = StringComparison.Ordinal
            )
        {
            var last_star = wildcard.LastIndexOf( '*' );
            var last_question_mark = wildcard.LastIndexOf( '?' );
            var suffix = Math.Max( last_star, last_question_mark );
            var original_string = str;

            if( suffix == -1 )
            {
                return original_string.Equals( wildcard, comparison_method );
            }

            if( suffix + 1 < wildcard.Length )
            {
                var suffix_string = wildcard.Substring( suffix + 1 );
                if( !original_string.EndsWith( suffix_string, comparison_method ) )
                {
                    return false;
                }

                wildcard = wildcard.Substring( 0, suffix + 1 );
                original_string = original_string.Substring( 0, original_string.Length - suffix_string.Length );
            }

            var first_star = wildcard.IndexOf( '*' );
            var first_question = wildcard.IndexOf( '?' );
            var prefix = Math.Min( first_star < 0 ? int.MaxValue : first_star,
                first_question < 0 ? int.MaxValue : first_question );

            Debug.Assert( prefix >= 0 && prefix < wildcard.Length );

            if( prefix > 0 )
            {
                var prefix_string = wildcard.Substring( 0, prefix );
                if( !original_string.StartsWith( prefix_string, comparison_method ) )
                {
                    return false;
                }

                wildcard = wildcard.Substring( prefix );
                original_string = original_string.Substring( prefix );
            }

            Debug.Assert( wildcard.Length > 0 );
            var first_wildcard = wildcard[ 0 ];
            wildcard = wildcard.Substring( 1 );
            if( first_wildcard == '*' || first_wildcard == '?' )
            {
                if( wildcard.Length == 0 )
                {
                    if( first_wildcard == '*' || original_string.Length < 2 )
                    {
                        return true;
                    }
                }

                var max_iterations = Math.Min( original_string.Length, first_wildcard == '?' ? 1 : int.MaxValue );
                for( var i = 0; i < max_iterations; ++i )
                {
                    if( original_string.Substring( i ).MatchWildcard( wildcard, comparison_method ) )
                    {
                        return true;
                    }
                }

                return false;
            }

            Debug.Assert( false, "Should not get here" );
            return false;
        }
    }
}