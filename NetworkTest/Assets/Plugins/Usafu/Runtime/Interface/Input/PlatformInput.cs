using System.Threading.Tasks;

namespace FishingCactus.Input
{
    public delegate void OnVirtualKeyboardDisplayChangedDelegate( bool is_displayed );
    
    public interface IPlatformInput
    {
        event OnVirtualKeyboardDisplayChangedDelegate OnVirtualKeyboardDisplayChanged;

        bool MustHandleVirtualKeyboard { get; }
        bool IsVirtualKeyboardDisplayed { get; } 
        void Initialize( Setup.Settings settings );
        Task< string > GetVirtualKeyboardTextAsync( string default_text, string title, string description );
        void CancelVirtualKeyboard();
    }

    public abstract class PlatformInputBase : IPlatformInput
    {
#pragma warning disable CS0067 // The event 'event' is never used
        public event OnVirtualKeyboardDisplayChangedDelegate OnVirtualKeyboardDisplayChanged;
#pragma warning restore CS0067 // The event 'event' is never used
    
        public virtual bool MustHandleVirtualKeyboard { get => false; }
        public virtual bool IsVirtualKeyboardDisplayed { get => false; } 

        public virtual void Initialize( Setup.Settings settings )
        {
        }

        public virtual Task< string > GetVirtualKeyboardTextAsync( string default_text, string title, string description )
        {
            return Task.FromResult( string.Empty );
        }

        protected void InvokeOnVirtualKeyboardDisplayChanged( bool is_displayed )
        {
            OnVirtualKeyboardDisplayChanged?.Invoke( is_displayed );
        }

        public virtual void CancelVirtualKeyboard()
        {
        }
    }
}