# Basements

This is a project to mantain the Basements mod created by rolopogo and mantained by sbtoonz for some time.

## Developing

This project was created with subreferences, I'll leave it as it is now.

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