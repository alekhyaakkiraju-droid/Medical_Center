import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { NppContent } from './npp.service';

export interface NppModalRequest {
  content: NppContent;
  resolve: (acknowledged: boolean) => void;
}

@Injectable({
  providedIn: 'root',
})
export class NppModalService {
  private readonly requestSubject = new Subject<NppModalRequest>();
  readonly request$ = this.requestSubject.asObservable();

  show(content: NppContent): Promise<boolean> {
    return new Promise((resolve) => {
      this.requestSubject.next({ content, resolve });
    });
  }
}
