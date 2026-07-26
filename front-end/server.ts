/**
 * WO-060: Validates Angular 22 SSR compatibility. AngularNodeAppEngine requires server route
 * manifest wiring that returns null for NgModule-less apps; CommonEngine remains functional in
 * v22 with standalone BootstrapContext bootstrap until route discovery is configured.
 * Express 5 route patterns and error handling align with WOREF-058/059 dependencies.
 */
import { APP_BASE_HREF } from '@angular/common';
import {
  CommonEngine,
  createNodeRequestHandler,
  isMainModule,
} from '@angular/ssr/node';
import express from 'express';
import { join } from 'node:path';
import bootstrap from './src/main.server';

const browserDistFolder = join(import.meta.dirname, '../browser');
const indexHtml = join(import.meta.dirname, 'index.server.html');
const ssrAllowedHosts = ['localhost', 'localhost:4000', '127.0.0.1', '127.0.0.1:4000'];

const app = express();
const commonEngine = new CommonEngine({
  allowedHosts: ssrAllowedHosts,
});

app.set('view engine', 'html');
app.set('views', browserDistFolder);

app.use(
  express.static(browserDistFolder, {
    maxAge: '1y',
    index: false,
    redirect: false,
  }),
);

app.all('/api/{*path}', (_req, res) => {
  res.status(404).json({ error: 'API endpoint not found' });
});

app.use((req, res, next) => {
  const { protocol, originalUrl, baseUrl, headers } = req;

  commonEngine
    .render({
      bootstrap,
      documentFilePath: indexHtml,
      url: `${protocol}://${headers.host}${originalUrl}`,
      publicPath: browserDistFolder,
      providers: [{ provide: APP_BASE_HREF, useValue: baseUrl }],
    })
    .then((html) => res.send(html))
    .catch(next);
});

app.use(
  (
    err: unknown,
    _req: express.Request,
    res: express.Response,
    next: express.NextFunction,
  ) => {
    console.error(
      'SSR rendering error:',
      err instanceof Error ? err.message : err,
    );
    if (res.headersSent) {
      return next(err);
    }
    res.status(500).send('Internal Server Error');
  },
);

if (isMainModule(import.meta.url) || process.env['pm_id']) {
  const port = process.env['PORT'] || 4000;
  app.listen(port, () => {
    console.log(`Node Express server listening on http://localhost:${port}`);
  });
}

export default createNodeRequestHandler(app);
