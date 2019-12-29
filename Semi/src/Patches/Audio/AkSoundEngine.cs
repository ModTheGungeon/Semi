using System;
using MonoMod;
using UnityEngine;

namespace Semi {
	[MonoModPatch("global::AkSoundEngine")]
	public class AkSoundEngine : global::AkSoundEngine {
		public static extern uint orig_PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfo in_pExternalSources, uint in_PlayingID);
		public static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfo in_pExternalSources, uint in_PlayingID) {
			WWiseAudioBridge.Logger.Debug($"in_eventID = {in_eventID}, in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags} in_pfnCallback = {in_pfnCallback} in_pCookie = {in_pCookie} in_cExternals = {in_cExternals} in_pExternalSources = {in_pExternalSources} in_PlayingID = {in_PlayingID}");
			return orig_PostEvent(in_eventID, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals, in_pExternalSources, in_PlayingID);
		}

		public static extern uint orig_PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfo in_pExternalSources);
		public static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfo in_pExternalSources) {
			WWiseAudioBridge.Logger.Debug($"in_eventID = {in_eventID}, in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags} in_pfnCallback = {in_pfnCallback} in_pCookie = {in_pCookie} in_cExternals = {in_cExternals} in_pExternalSources = {in_pExternalSources}");
			return orig_PostEvent(in_eventID, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals, in_pExternalSources);
		}

		public static extern uint orig_PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals);
		public static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals) {
			WWiseAudioBridge.Logger.Debug($"in_eventID = {in_eventID}, in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags} in_pfnCallback = {in_pfnCallback} in_pCookie = {in_pCookie} in_cExternals = {in_cExternals}");
			return orig_PostEvent(in_eventID, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals);
		}

		public static extern uint orig_PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie);
		public static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie) {
			WWiseAudioBridge.Logger.Debug($"in_eventID = {in_eventID}, in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags} in_pfnCallback = {in_pfnCallback} in_pCookie = {in_pCookie}");
			return orig_PostEvent(in_eventID, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie);
		}

		public static extern uint orig_PostEvent(string in_pszEventName, GameObject in_gameObjectID);
		public static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID) {
			WWiseAudioBridge.Logger.Debug($"in_pszEventName = '{in_pszEventName}' in_gameObjectID = {in_gameObjectID}");
			WWiseAudioBridge.Logger.Debug($"String key, patched");
			var name = WWiseAudioBridge.PostEventOrReturnWWiseEventName((ID)in_pszEventName.ToLowerInvariant(), in_gameObjectID);
			if (name == null) return 0;
			return orig_PostEvent(in_pszEventName, in_gameObjectID);
		}

		public static extern uint orig_PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags);
		public static uint PostEvent(uint in_eventID, GameObject in_gameObjectID, uint in_uFlags) {
			WWiseAudioBridge.Logger.Debug($"in_eventID = {in_eventID}, in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags}");
			return orig_PostEvent(in_eventID, in_gameObjectID, in_uFlags);
		}

		public static extern uint orig_PostEvent(uint in_eventID, GameObject in_gameObjectID);
		public static uint PostEvent(uint in_eventID, GameObject in_gameObjectID) {
			WWiseAudioBridge.Logger.Debug($"in_eventID = {in_eventID}, in_gameObjectID = {in_gameObjectID}");
			return orig_PostEvent(in_eventID, in_gameObjectID);
		}

		public static extern uint orig_PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie);
		public static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie) {
			WWiseAudioBridge.Logger.Debug($"in_pszEventName = '{in_pszEventName}' in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags}, in_pfnCallback = {in_pfnCallback}, in_pCookie = {in_pCookie}");
			WWiseAudioBridge.Logger.Debug($"String key, patched");
			var name = WWiseAudioBridge.PostEventOrReturnWWiseEventName((ID)in_pszEventName.ToLowerInvariant(), in_gameObjectID);
			if (name == null) return 0;
			return orig_PostEvent(in_pszEventName, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie);
		}

		public static extern uint orig_PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags);
		public static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags) {
			WWiseAudioBridge.Logger.Debug($"in_pszEventName = '{in_pszEventName}' in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags}");
			WWiseAudioBridge.Logger.Debug($"String key, patched");
			var name = WWiseAudioBridge.PostEventOrReturnWWiseEventName((ID)in_pszEventName.ToLowerInvariant(), in_gameObjectID);
			if (name == null) return 0;
			return orig_PostEvent(in_pszEventName, in_gameObjectID, in_uFlags);
		}

		public static extern uint orig_PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfo in_pExternalSources, uint in_PlayingID);
		public static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfo in_pExternalSources, uint in_PlayingID) {
			WWiseAudioBridge.Logger.Debug($"in_pszEventName = '{in_pszEventName}' in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags}, in_pfnCallback = {in_pfnCallback}, in_pCookie = {in_pCookie}, in_cExternals = {in_cExternals}, in_pExternalSources = {in_pExternalSources}, in_PlayingID = {in_PlayingID}");
			WWiseAudioBridge.Logger.Debug($"String key, patched");
			var name = WWiseAudioBridge.PostEventOrReturnWWiseEventName((ID)in_pszEventName.ToLowerInvariant(), in_gameObjectID);
			if (name == null) return 0;
			return orig_PostEvent(in_pszEventName, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals, in_pExternalSources, in_PlayingID);
		}

		public static extern uint orig_PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfo in_pExternalSources);
		public static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals, AkExternalSourceInfo in_pExternalSources) {
			WWiseAudioBridge.Logger.Debug($"in_pszEventName = '{in_pszEventName}' in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags}, in_pfnCallback = {in_pfnCallback}, in_pCookie = {in_pCookie}, in_cExternals = {in_cExternals}, in_pExternalSources = {in_pExternalSources}");
			WWiseAudioBridge.Logger.Debug($"String key, patched");
			var name = WWiseAudioBridge.PostEventOrReturnWWiseEventName((ID)in_pszEventName.ToLowerInvariant(), in_gameObjectID);
			if (name == null) return 0;
			return orig_PostEvent(in_pszEventName, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals, in_pExternalSources);
		}

		public static extern uint orig_PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals);
		public static uint PostEvent(string in_pszEventName, GameObject in_gameObjectID, uint in_uFlags, AkCallbackManager.EventCallback in_pfnCallback, object in_pCookie, uint in_cExternals) {
			WWiseAudioBridge.Logger.Debug($"in_pszEventName = '{in_pszEventName}' in_gameObjectID = {in_gameObjectID}, in_uFlags = {in_uFlags}, in_pfnCallback = {in_pfnCallback}, in_pCookie = {in_pCookie}, in_cExternals = {in_cExternals}");
			WWiseAudioBridge.Logger.Debug($"String key, patched");
			var name = WWiseAudioBridge.PostEventOrReturnWWiseEventName((ID)in_pszEventName.ToLowerInvariant(), in_gameObjectID);
			if (name == null) return 0;
			return orig_PostEvent(in_pszEventName, in_gameObjectID, in_uFlags, in_pfnCallback, in_pCookie, in_cExternals);
		}
	}
}