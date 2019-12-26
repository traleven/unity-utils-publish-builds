# unity-utils-publish-builds
A simple Editor tool to automatically publish new Unity builds to Slack channel

# Usage
Put slack.json file into your Unity project root:
```json
{
	"oauthToken": "your OAuth token for Slack app with 'files:write:user' and 'remote_files:share' permissions", 
	"uploadEndpoint": "https://slack.com/api/files.upload", 
	"channel": "channel id you want to post your build to"
}
```
You can add this file to your .gitignore to keep your OAuth token safely out of repository.

# Dependencies
- [unity-utils-editor-coroutine](https://github.com/traleven/unity-utils-editor-coroutine) makes possible to use UnityWebRequest in Editor.
- [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) packs the build into zip archive.