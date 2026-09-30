// MiniShotMenu - Win11 一级右键菜单「粘贴截图」
// IExplorerCommand，点击后调用同目录的 MiniShot.exe --paste "<当前文件夹>"
#include <windows.h>
#include <shobjidl_core.h>
#include <shlobj.h>
#include <shlwapi.h>

// {6B1E3F52-9C1A-4F8E-A7D2-3C5B9E4A1F70}
static const CLSID CLSID_PasteShot = { 0x6b1e3f52, 0x9c1a, 0x4f8e, { 0xa7, 0xd2, 0x3c, 0x5b, 0x9e, 0x4a, 0x1f, 0x70 } };

// 不链接 C 运行库，自己提供 new/delete
void* operator new(size_t n) { return HeapAlloc(GetProcessHeap(), 0, n); }
void operator delete(void* p) { HeapFree(GetProcessHeap(), 0, p); }
void operator delete(void* p, size_t) { HeapFree(GetProcessHeap(), 0, p); }
extern "C" int __cdecl _purecall() { return 0; }
#pragma function(memset, memcmp)
extern "C" void* __cdecl memset(void* d, int c, size_t n) { unsigned char* p = (unsigned char*)d; while (n--) *p++ = (unsigned char)c; return d; }
extern "C" int __cdecl memcmp(const void* a, const void* b, size_t n)
{
    const unsigned char *x = (const unsigned char*)a, *y = (const unsigned char*)b;
    for (; n; n--, x++, y++) if (*x != *y) return *x - *y;
    return 0;
}

static HMODULE g_module;
static LONG g_refs;

static void ExePath(wchar_t* buf, DWORD len)
{
    GetModuleFileNameW(g_module, buf, len);
    PathRemoveFileSpecW(buf);
    PathAppendW(buf, L"MiniShot.exe");
}

// 背景右键时没有选中项，从资源管理器窗口取当前文件夹
static bool FolderFromSite(IUnknown* site, wchar_t* out)
{
    bool ok = false;
    IServiceProvider* sp = nullptr;
    if (!site || FAILED(site->QueryInterface(IID_PPV_ARGS(&sp)))) return false;
    IShellBrowser* sb = nullptr;
    if (SUCCEEDED(sp->QueryService(SID_STopLevelBrowser, IID_PPV_ARGS(&sb))))
    {
        IShellView* sv = nullptr;
        if (SUCCEEDED(sb->QueryActiveShellView(&sv)))
        {
            IFolderView* fv = nullptr;
            if (SUCCEEDED(sv->QueryInterface(IID_PPV_ARGS(&fv))))
            {
                IShellItem* si = nullptr;
                if (SUCCEEDED(fv->GetFolder(IID_PPV_ARGS(&si))))
                {
                    PWSTR p = nullptr;
                    if (SUCCEEDED(si->GetDisplayName(SIGDN_FILESYSPATH, &p)))
                    {
                        lstrcpynW(out, p, MAX_PATH);
                        CoTaskMemFree(p);
                        ok = true;
                    }
                    si->Release();
                }
                fv->Release();
            }
            sv->Release();
        }
        sb->Release();
    }
    sp->Release();
    return ok;
}

class PasteShot : public IExplorerCommand, public IObjectWithSite
{
    LONG refs = 1;
    IUnknown* site = nullptr;
public:
    PasteShot() { InterlockedIncrement(&g_refs); }
    ~PasteShot() { if (site) site->Release(); InterlockedDecrement(&g_refs); }

    IFACEMETHODIMP QueryInterface(REFIID riid, void** ppv)
    {
        if (riid == IID_IUnknown || riid == IID_IExplorerCommand) *ppv = static_cast<IExplorerCommand*>(this);
        else if (riid == IID_IObjectWithSite) *ppv = static_cast<IObjectWithSite*>(this);
        else { *ppv = nullptr; return E_NOINTERFACE; }
        AddRef();
        return S_OK;
    }
    IFACEMETHODIMP_(ULONG) AddRef() { return InterlockedIncrement(&refs); }
    IFACEMETHODIMP_(ULONG) Release()
    {
        LONG r = InterlockedDecrement(&refs);
        if (!r) delete this;
        return r;
    }

    // IObjectWithSite
    IFACEMETHODIMP SetSite(IUnknown* s)
    {
        if (site) site->Release();
        site = s;
        if (site) site->AddRef();
        return S_OK;
    }
    IFACEMETHODIMP GetSite(REFIID riid, void** ppv)
    {
        if (!site) { *ppv = nullptr; return E_FAIL; }
        return site->QueryInterface(riid, ppv);
    }

    // IExplorerCommand
    IFACEMETHODIMP GetTitle(IShellItemArray*, LPWSTR* name) { return SHStrDupW(L"粘贴截图", name); }
    IFACEMETHODIMP GetIcon(IShellItemArray*, LPWSTR* icon)
    {
        wchar_t p[MAX_PATH + 4];
        ExePath(p, MAX_PATH);
        lstrcatW(p, L",0");
        return SHStrDupW(p, icon);
    }
    IFACEMETHODIMP GetToolTip(IShellItemArray*, LPWSTR* tip) { *tip = nullptr; return E_NOTIMPL; }
    IFACEMETHODIMP GetCanonicalName(GUID* g) { *g = CLSID_PasteShot; return S_OK; }
    IFACEMETHODIMP GetState(IShellItemArray*, BOOL, EXPCMDSTATE* state) { *state = ECS_ENABLED; return S_OK; }
    IFACEMETHODIMP GetFlags(EXPCMDFLAGS* flags) { *flags = ECF_DEFAULT; return S_OK; }
    IFACEMETHODIMP EnumSubCommands(IEnumExplorerCommand** e) { *e = nullptr; return E_NOTIMPL; }

    IFACEMETHODIMP Invoke(IShellItemArray* items, IBindCtx*)
    {
        wchar_t folder[MAX_PATH] = L"";
        bool ok = false;
        DWORD n = 0;
        if (items && SUCCEEDED(items->GetCount(&n)) && n > 0)
        {
            IShellItem* si = nullptr;
            if (SUCCEEDED(items->GetItemAt(0, &si)))
            {
                PWSTR p = nullptr;
                if (SUCCEEDED(si->GetDisplayName(SIGDN_FILESYSPATH, &p)))
                {
                    lstrcpynW(folder, p, MAX_PATH);
                    CoTaskMemFree(p);
                    ok = true;
                }
                si->Release();
            }
        }
        if (!ok) ok = FolderFromSite(site, folder);
        if (!ok) return E_FAIL;

        wchar_t exe[MAX_PATH];
        ExePath(exe, MAX_PATH);
        PathRemoveBackslashW(folder); // 避免 "C:\" 结尾的反斜杠吃掉引号
        wchar_t cmd[MAX_PATH * 2 + 32];
        wsprintfW(cmd, L"\"%s\" --paste \"%s\"", exe, folder);
        STARTUPINFOW si = { sizeof(si) };
        PROCESS_INFORMATION pi;
        if (!CreateProcessW(exe, cmd, nullptr, nullptr, FALSE, 0, nullptr, folder, &si, &pi)) return HRESULT_FROM_WIN32(GetLastError());
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        return S_OK;
    }
};

class Factory : public IClassFactory
{
public:
    IFACEMETHODIMP QueryInterface(REFIID riid, void** ppv)
    {
        if (riid == IID_IUnknown || riid == IID_IClassFactory) { *ppv = this; return S_OK; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    IFACEMETHODIMP_(ULONG) AddRef() { return 2; }
    IFACEMETHODIMP_(ULONG) Release() { return 1; }
    IFACEMETHODIMP CreateInstance(IUnknown* outer, REFIID riid, void** ppv)
    {
        *ppv = nullptr;
        if (outer) return CLASS_E_NOAGGREGATION;
        PasteShot* p = new PasteShot();
        if (!p) return E_OUTOFMEMORY;
        HRESULT hr = p->QueryInterface(riid, ppv);
        p->Release();
        return hr;
    }
    IFACEMETHODIMP LockServer(BOOL lock)
    {
        if (lock) InterlockedIncrement(&g_refs); else InterlockedDecrement(&g_refs);
        return S_OK;
    }
};

static Factory g_factory;

BOOL WINAPI DllMain(HINSTANCE h, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH) { g_module = h; DisableThreadLibraryCalls(h); }
    return TRUE;
}

STDAPI DllGetClassObject(REFCLSID clsid, REFIID riid, void** ppv)
{
    if (clsid != CLSID_PasteShot) { *ppv = nullptr; return CLASS_E_CLASSNOTAVAILABLE; }
    return g_factory.QueryInterface(riid, ppv);
}

STDAPI DllCanUnloadNow() { return g_refs == 0 ? S_OK : S_FALSE; }
