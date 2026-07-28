import { InjectionToken } from '@angular/core';

export interface IdleTimeoutConfig {
  idleDurationMs: number;
  warningDurationMs: number;
}

export const DEFAULT_IDLE_TIMEOUT_CONFIG: IdleTimeoutConfig = {
  idleDurationMs: 840_000,
  warningDurationMs: 60_000,
};

export const IDLE_TIMEOUT_CONFIG = new InjectionToken<IdleTimeoutConfig>(
  'IDLE_TIMEOUT_CONFIG',
  {
    factory: () => DEFAULT_IDLE_TIMEOUT_CONFIG,
  }
);
