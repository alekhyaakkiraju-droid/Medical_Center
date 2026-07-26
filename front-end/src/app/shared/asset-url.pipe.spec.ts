import { AssetUrlPipe } from './asset-url.pipe';
import { environment } from '../../environments/environment';

describe('AssetUrlPipe', () => {
  const pipe = new AssetUrlPipe();
  const originalAssetBaseUrl = environment.assetBaseUrl;

  afterEach(() => {
    environment.assetBaseUrl = originalAssetBaseUrl;
  });

  it('returns root-relative path when asset base URL is empty', () => {
    environment.assetBaseUrl = '';
    expect(pipe.transform('images/team/event-1.jpg')).toBe('/images/team/event-1.jpg');
    expect(pipe.transform('/images/team/event-1.jpg')).toBe('/images/team/event-1.jpg');
  });

  it('prepends CDN base URL when configured', () => {
    environment.assetBaseUrl = 'https://cdn.example.com/assets';
    expect(pipe.transform('images/gallery/gallery-01.jpg')).toBe(
      'https://cdn.example.com/assets/images/gallery/gallery-01.jpg'
    );
  });

  it('deduplicates trailing slash on base URL', () => {
    environment.assetBaseUrl = 'https://cdn.example.com/assets/';
    expect(pipe.transform('/images/icons/logo.png')).toBe(
      'https://cdn.example.com/assets/images/icons/logo.png'
    );
  });

  it('passes through absolute external URLs unchanged', () => {
    environment.assetBaseUrl = 'https://cdn.example.com';
    expect(pipe.transform('https://example.com/photo.jpg')).toBe('https://example.com/photo.jpg');
    expect(pipe.transform('data:image/png;base64,abc')).toBe('data:image/png;base64,abc');
  });
});
