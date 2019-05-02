using System.Collections.Generic;

namespace Core.Data
{
	/// <summary>
	/// Base game data store for GameManager to save, containing only data for saving volumes
	/// </summary>
	public abstract class GameDataStoreBase : IDataStore
	{
		public float masterVolume = 1;

		public float sfxVolume = 1;

		public float musicVolume = 1;
		
		// list of achievements we know we have unlocked (to avoid making repeated calls to the API)
		public Dictionary<string,bool> mUnlockedAchievements = new Dictionary<string, bool>();

		// achievement increments we are accumulating locally, waiting to send to the games API
		public Dictionary<string,int> mPendingIncrements = new Dictionary<string, int>();

		/// <summary>
		/// Called just before we save
		/// </summary>
		public abstract void PreSave();

		/// <summary>
		/// Called just after load
		/// </summary>
		public abstract void PostLoad();
	}
}