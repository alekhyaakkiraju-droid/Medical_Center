import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';

declare global {
  interface Window {
    grecaptcha?: {
      ready: (callback: () => void) => void;
      execute: (siteKey: string, options: { action: string }) => Promise<string>;
    };
  }
}

@Injectable({
  providedIn: 'root'
})
export class RecaptchaService {
  private scriptLoadPromise?: Promise<void>;

  execute(action: string): Promise<string> {
    const siteKey = environment.recaptchaSiteKey;
    if (!siteKey) {
      return Promise.resolve('');
    }

    return this.ensureScriptLoaded(siteKey).then(
      () =>
        new Promise<string>((resolve, reject) => {
          window.grecaptcha?.ready(() => {
            window.grecaptcha
              ?.execute(siteKey, { action })
              .then(resolve)
              .catch(reject);
          });
        })
    );
  }

  private ensureScriptLoaded(siteKey: string): Promise<void> {
    if (window.grecaptcha) {
      return Promise.resolve();
    }

    if (!this.scriptLoadPromise) {
      this.scriptLoadPromise = new Promise<void>((resolve, reject) => {
        const existingScript = document.querySelector<HTMLScriptElement>(
          'script[data-recaptcha-v3="true"]'
        );

        if (existingScript) {
          existingScript.addEventListener('load', () => resolve(), { once: true });
          existingScript.addEventListener('error', () => reject(new Error('Failed to load reCAPTCHA.')), {
            once: true
          });
          return;
        }

        const script = document.createElement('script');
        script.src = `https://www.google.com/recaptcha/api.js?render=${siteKey}`;
        script.async = true;
        script.defer = true;
        script.dataset['recaptchaV3'] = 'true';
        script.onload = () => resolve();
        script.onerror = () => reject(new Error('Failed to load reCAPTCHA.'));
        document.head.appendChild(script);
      });
    }

    return this.scriptLoadPromise;
  }
}
