#include <stdio.h>

#ifdef _WIN32
#include <windows.h>
#endif

#ifdef __linux__
#include <stdlib.h>
#endif

#ifdef __APPLE__
#include <stdlib.h>
#endif

void create_popup(const char *message) {
#ifdef _WIN32
    MessageBox(NULL, message, "Aviso", MB_OK | MB_ICONINFORMATION);
#endif

#ifdef __linux__
    char command[256];
    snprintf(command, sizeof(command), "zenity --info --text='%s'", message);
    system(command);
#endif

#ifdef __APPLE__
    char command[256];
    snprintf(command, sizeof(command), "osascript -e 'display dialog \"%s\" with title \"Aviso\"'", message);
    system(command);
#endif
}
