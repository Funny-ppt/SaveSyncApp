namespace SaveSyncApp;

public interface IProfileProvider
{
    bool TryGetProfile(out Profile profile);
    bool TrySaveProfile(Profile profile);
}
