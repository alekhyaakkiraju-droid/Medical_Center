/**
 * WO-060: Angular 22 SSR uses AngularNodeAppEngine (CommonEngine deprecated in v22).
 * Express 5 route patterns and error handling align with WOREF-058/059 dependencies.
 */
import {
  AngularNodeAppEngine,
  createNodeRequestHandler,
  isMainModule,
  writeResponseToNodeResponse,
} from '@angular/ssr/node';
import express from 'express';
import { join } from 'node:path';

const browserDistFolder = join(import.meta.dirname, '../browser');

const app = express();
const angularApp = new AngularNodeAppEngine();

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
  angularApp
    .handle(req)
    .then((response) =>
      response ? writeResponseToNodeResponse(response, res) : next(),
    )
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
