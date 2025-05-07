
        private async Task<bool> CheckForUpdate()
        {
            try
            {
                Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                Version remoteVersion;

                using (WebClient client = new WebClient())
                {
                    string remoteVersionText = await client.DownloadStringTaskAsync("https://raw.githubusercontent.com/THENooBlet/TCFInventoryBoss/main/version.txt");
                    if (!Version.TryParse(remoteVersionText.Trim(), out remoteVersion))
                        throw new FormatException("Remote version string is not a valid version.");

                    if (currentVersion < remoteVersion)
                    {
                        DialogResult result = MessageBox.Show(
                            $"A new version ({remoteVersion}) is available. You must update to continue.\n\nDo you want to download it now?",
                            "Update Required",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            string zipUrl = "https://github.com/THENooBlet/TCFInventoryBoss/releases/latest/download/TCFInventoryBoss.zip";
                            string zipPath = Path.Combine(Path.GetTempPath(), "TCFInventoryBoss_Update.zip");
                            await DownloadUpdateWithProgress(zipUrl, zipPath);
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update check failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private TaskCompletionSource<bool> downloadCompletedTcs;

        private async Task DownloadUpdateWithProgress(string zipUrl, string zipPath)
        {
            using (WebClient client = new WebClient())
            {
                downloadCompletedTcs = new TaskCompletionSource<bool>();

                Invoke((Action)(() =>
                {
                    downloadProgressBar.Visible = true;
                    downloadProgressBar.Value = 0;
                }));

                client.DownloadProgressChanged += (s, e) =>
                {
                    Invoke((Action)(() =>
                    {
                        downloadProgressBar.Value = e.ProgressPercentage;
                    }));
                };

                client.DownloadFileCompleted += (s, e) =>
                {
                    Invoke((Action)(() =>
                    {
                        downloadProgressBar.Visible = false;

                        if (e.Error != null)
                        {
                            MessageBox.Show("Download failed:\n" + e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            downloadCompletedTcs.SetResult(false);
                            return;
                        }

                        try
                        {
                            string appDir = AppDomain.CurrentDomain.BaseDirectory;
                            string exePath = Application.ExecutablePath;
                            string updaterPath = Path.Combine(appDir, "Updater.exe");

                            if (!File.Exists(updaterPath))
                            {
                                MessageBox.Show("Updater.exe not found in application directory.", "Error");
                                return;
                            }

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = updaterPath,
                                Arguments = $"{zipPath} {appDir} {exePath}",
                                UseShellExecute = true
                            });

                            Application.Exit();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Could not launch updater:\n" + ex.Message, "Error");
                        }

                        downloadCompletedTcs.SetResult(true);
                    }));
                };

                try
                {
                    client.DownloadFileAsync(new Uri(zipUrl), zipPath);
                    await downloadCompletedTcs.Task;
                }
                catch (Exception ex)
                {
                    Invoke((Action)(() =>
                    {
                        downloadProgressBar.Visible = false;
                        MessageBox.Show("Error during download:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            }
        }

