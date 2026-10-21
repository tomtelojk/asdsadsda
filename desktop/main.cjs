const { app, BrowserWindow, Menu, session } = require('electron');
const path = require('node:path');
if (!app.requestSingleInstanceLock()) app.quit();
let win;
app.on('second-instance', () => { if (win) { if (win.isMinimized()) win.restore(); win.focus(); } });
app.whenReady().then(() => {
  session.defaultSession.setPermissionRequestHandler((_contents, _permission, callback) => callback(false));
  win = new BrowserWindow({
    title: 'Vale do Sol', width: 1440, height: 960, minWidth: 960, minHeight: 700,
    backgroundColor: '#263c32', show: false,
    webPreferences: { nodeIntegration: false, contextIsolation: true, sandbox: true, webSecurity: true }
  });
  win.webContents.setWindowOpenHandler(() => ({ action: 'deny' }));
  win.webContents.on('will-navigate', e => e.preventDefault());
  Menu.setApplicationMenu(Menu.buildFromTemplate([
    { label: 'Jogo', submenu: [{ label: 'Tela cheia', accelerator: 'F11', click: () => win.setFullScreen(!win.isFullScreen()) }, { type: 'separator' }, { role: 'quit', label: 'Sair' }] },
    { label: 'Exibir', submenu: [{ role: 'resetZoom', label: 'Tamanho original' }, { role: 'zoomIn', label: 'Aumentar' }, { role: 'zoomOut', label: 'Diminuir' }] }
  ]));
  win.once('ready-to-show', () => win.show());
  win.loadFile(path.join(__dirname, '..', 'dist', 'index.html'));
});
app.on('window-all-closed', () => { if (process.platform !== 'darwin') app.quit(); });
