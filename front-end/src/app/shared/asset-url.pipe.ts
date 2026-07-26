import { Pipe, PipeTransform } from '@angular/core';
import { resolveAssetUrl } from './asset-url.util';

@Pipe({
  name: 'assetUrl',
})
export class AssetUrlPipe implements PipeTransform {
  transform(path: string | null | undefined): string {
    return resolveAssetUrl(path);
  }
}
