using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.Callbacks;

public static class SlackPost
{
	[System.Serializable]
	public struct SlackCredentials
	{
		public string oauthToken;
		public string uploadEndpoint;
		public string channel;
	}

	[PostProcessBuild(1010)]
	public static void PostToSlack(BuildTarget target, string pathToBuiltProject)
	{
		SlackCredentials credentials = JsonUtility.FromJson<SlackCredentials>(File.ReadAllText("slack.json"));

		string version = PlayerSettings.bundleVersion;
		string buildGuid = GUID.Generate().ToString().Substring(0, 6);
		string pathToBuild = Path.GetDirectoryName(pathToBuiltProject);
		string dir = Path.GetDirectoryName(pathToBuild);
		string zipName = Path.Combine(dir, Path.GetFileName(pathToBuild) + ".zip");

		if (File.Exists(zipName))
		{
			File.Delete(zipName);
		}

		var archive = new ICSharpCode.SharpZipLib.Zip.FastZip();
		{
			archive.CreateEmptyDirectories = true;
			archive.RestoreAttributesOnExtract = true;
			archive.RestoreDateTimeOnExtract = true;
			archive.CreateZip(zipName, pathToBuild, true, "");
		}

		EditorCoroutine.StartCoroutine(SendWebRequest(
			credentials,
			zipName,
			version + "." + buildGuid,
			$"New {target} build {version}"));
	}

	private static IEnumerator SendWebRequest(SlackCredentials credentials, string zipName, string version, string message)
	{
		UnityWebRequest uploadRequest = UnityWebRequest.Post(credentials.uploadEndpoint, new List<IMultipartFormSection> {
			new MultipartFormFileSection("file", File.ReadAllBytes(zipName), $"@{zipName}", "application/zip"),
			new MultipartFormDataSection("filename", $"{Path.GetFileNameWithoutExtension(zipName)}.{version}{Path.GetExtension(zipName)}"),
			new MultipartFormDataSection("title", $"{Path.GetFileNameWithoutExtension(zipName)}.{version}"),
			new MultipartFormDataSection("channels", credentials.channel),
			new MultipartFormDataSection("initial_comment", message),
		});

		uploadRequest.SetRequestHeader("Authorization", $"Bearer {credentials.oauthToken}");


		Debug.Log($"Uploading {zipName}");
		uploadRequest.SendWebRequest();

		while (!uploadRequest.isDone)
			yield return null;

		if (uploadRequest.isNetworkError)
			Debug.Log(uploadRequest.error);
		else
			Debug.Log($"Posted to Slack\n{uploadRequest.downloadHandler.text}");
	}

	[MenuItem("Lucid/Post to Slack")]
    public static void PostToSlack()
	{
		string filePath = Path.Combine("/Users/diplomat/Projects/Lucid/Nextmotion/NRS", "Builds", "NRS-Linux", "NRS-Main.x86_64");
		PostToSlack(BuildTarget.StandaloneLinux64, filePath);
	}
}
