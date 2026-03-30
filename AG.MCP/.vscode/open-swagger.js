const { execSync } = require('child_process');
execSync(
  'cmd /c start "" "http://localhost:57657/swagger" && start "" "https://localhost:57655/swagger"'
);
