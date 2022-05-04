# Telegram Document Downloader

This will take a given group name and login as you using the client api (not a bot)
and download all of the documents in a group while removing duplicates.

It will create a small local sqlite database to keep track of files it has already
downloaded as well as files it has removed as a duplicate

This means that if you remove the files from the folder (Ie organized them) it wont
re-download them again so each run will only download new files.

# Instructions

To use you only have to run the cli program in its own folder so it can create its config and database files.  It will prompt you for your phonenumber in
+1xxxxxxx format, then ask for the folder you would like to download to, then the name of the group
as displayed in telegram to download from.  Just follow the prompts and enter the requested info.

Once you answer all the prompts it will save a config.yaml file to the folder the program is running
in as well as the telegram session file.  If you need to login again for some reason just delete the tg.session file