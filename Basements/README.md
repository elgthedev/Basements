# Basements

Basements is maintained by the OdinPlus team and preserves the original `com.rolopogo.Basement` plugin identity for existing players.

## Developing

The project uses the Valheim game assemblies plus the PieceManager and ServerSync submodules. Copy `environment.props.example` to `environment.props` and point it at your Valheim install and Gale debug profile before building.

Build Debug for local iteration. A Release build generates separate platform packages under `Publish/Thunderstore`, `Publish/Hexium`, and `Publish/Nexus`; inspect each ZIP before publishing. The Nexus archive contains only the merged mod DLL. Do not commit publicized game assemblies or loose dependency inputs.

### Client-side materials

The bundled Unity materials use the `_REPLACE_` prefix for PieceManager material swapping. Valheim 1.0 also requires the legacy `Heightmap_basematerial` to be replaced with the current `stonefloor` material. Basements applies these replacements to the registered prefab and again when a basement `Piece` awakens, so connected clients update the instance they render. Keep the prefix normalization when changing the material replacement code; matching the unprefixed game material name directly leaves the old material in place.

### Dependency refresh

When Valheim updates, refresh the publicized assemblies, verify the installed BepInExPack, and check the upstream [PieceManager](https://github.com/AzumattDev/PieceManager/releases) and [ServerSync](https://github.com/blaxxun-boop/ServerSync/releases) releases before rebuilding. Test first in a minimal Gale profile. Valheim 1.0 renamed `PieceTable`'s per-category available-piece collection to `m_availablePiecesByCategory`; Basements' embedded PieceManager is built from the current upstream source with that compatibility update until the upstream project publishes an official fix.

### Updating a Git Submodule in a "Detached HEAD" State

If you find that your submodule is in a "detached HEAD" state, follow these steps to update it to the latest commit on a specific branch.

It can be done for both references in the main project.

#### Steps to Update Submodule

1. **Navigate to the Submodule Directory**

   Open your terminal and navigate to the submodule directory.
   ```sh
   cd PieceManager
   ```

2. Check the Current Status

	Verify the current status of the submodule to confirm it is in a detached HEAD state.
	```sh
	git status
	```

3. Checkout the Desired Branch

	Checkout the branch you want the submodule to track.
	```sh
	git checkout master  # Replace 'master' with the branch you want to track
	```

4. Pull the Latest Changes

	Pull the latest changes from the submodule's repository.
	```sh
	git pull origin master  # Replace 'master' with the branch you are tracking
	```

5. Navigate Back to the Main Repository

	Return to the root directory of your main repository.
	```sh
	cd ..
	```

6. Commit the changes and push

## Licence

This project uses [MIT License](LICENSE)
