# Changelog

## [1.1.2]

### Updates
- Update comments (try to translate to English).

### Fixes
- Fix hidding of passwords for android's keystore

## [1.1.1]

### Improvements
- Update code structure.
- Code refactoring.

## [1.1.0]

### Features
- Custom naming for build output paths

### Improvements
- Update code structure.
- Code refactoring.

### NOTICES
- settings are not compatible with previous versions


## [1.0.4]

### Features
- Add android symbols.zip settings

### Fixes
- Fix Assembly Definitions


## [1.0.3]

### Features
- Update to upm
- Update android settings
- Add platform switcher

### Improvements
- Code refactoring.


## [1.0.2]

### Features
- Add more settings for Android (Scripting Backend, Target Architecture, Build Mode)


## [1.0.1]

### Features
- Update build number system.
- Update output build names.
- Add custom build.

### Improvements
- Code refactoring.


## [1.0.0]

### Features
- Override builder with json on command line.
- Memorize last selected builder each BuildTarget.

### Fixes
- `Enable/Disable scenes in build` is not reflected.


## [0.10.1]

### Fixes
- When select file path not under project directory, value to be empty.


## [0.10.0]

### Features
- Exclude directory in build.
- AssetBundle build report(Add/Delete/Update).


## [0.9.3]

### Fixes
- AssetBundle output path is incorrect.


## [0.9.2]

### Fixes
- Builder icon is not found.

## [0.9.1]

### Fixes
- Build target on edit multiple builder.

### Changes
- Rename `IPlatformSettings` to `IBuildTargetSettings`.


## [0.9.0]

### Features
- New editor window instead of inspector window.
- Copy AssetBundles to StreamingAssets.


## [0.8.1]

### Fixes
- Build button for AssetBundle is not displayed.


## [0.8.0]

### Features
- Build AssetBundle.
    * Supports options.
    * Build artifacts are generated in `<project_dir>/AssetBundles` directory.


## [0.7.0]

### Fixes
- 'Usage For CUI' was corrected.
- Util.projectDir has error after compiling.
- (iOS) When 'Automatically Sign' is enabled, ignore 'Provisioning Profile Id', etc...


## [0.6.0]

### Features
- Supports WebGL.
- Supports other platforms by implementing `IPlatformSettings` interface.


## [0.5.0]

### Features
- Supports Unity Cloud Build.
-Enable/Disable scenes in build.


## [0.4.0]

### Features
- (iOS) Supports language options for XCode.

### Obsoletes
- Obsolete 'Custom build pipeline' to simplify.
- Obsolete 'Generate CUI command' and explain it in Readme instead.
- Several CUI command options.
- Several BuildOptions.


## [0.3.0]

### Features
- (iOS) Supports XCode 8 & Automatically Sign.
- (iOS) Add framework options.
- (iOS) Add entitlement file.
- (iOS) Apple servises (iCloud, Push, GameCenter, etc...) can be enable.

### Fixes
- Inspector GUI.


## [0.2.0]

### Features
-  Add button to 'Build & Run'.
- (iOS) Generate 'exportOptions.plist'.

### Improvements
- Supports Unity5.5+.
- Improvement: Setting items are simplified. Several items have been deleted.

### Fixes
- Generated CUI command is incorrect.


## [0.1.0]

### Features
- Manage build configuration with builder asset.
- (Android) Keystore information.
- Custom build pipeline.
- Export `BUILD_VERSION` for CI.