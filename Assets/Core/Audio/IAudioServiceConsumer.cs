namespace ProjectZombie.Core.Audio
{
    /// <summary>Receives the audio port from the owning composition root.</summary>
    public interface IAudioServiceConsumer
    {
        void InjectAudioService(IAudioService audioService);
    }
}
