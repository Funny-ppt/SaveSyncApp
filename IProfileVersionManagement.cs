using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveSyncApp;

public interface IProfileVersionManagement
{
    int CurrentVersion { get; }
    Profile GetDefaultProfile();
    void UpdateToCurrentVersion(Profile profile);

}
