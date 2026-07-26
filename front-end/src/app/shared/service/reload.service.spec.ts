import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReloadService } from './reload.service';
import { standaloneComponentTestProviders } from '../../testing/standalone-component-test-providers';

describe('ReloadService', () => {
  let service: ReloadService;
  let container: HTMLDivElement;

  beforeEach(() => {
    container = document.createElement('div');
    document.body.appendChild(container);

    TestBed.configureTestingModule({
      providers: [...standaloneComponentTestProviders, ReloadService]
    });
    service = TestBed.inject(ReloadService);
  });

  afterEach(() => {
    container.remove();
  });

  function createPreloader(): { preloader: HTMLDivElement; loader: HTMLDivElement } {
    const preloader = document.createElement('div');
    preloader.id = 'preloader';
    preloader.style.display = 'block';

    const loader = document.createElement('div');
    loader.className = 'loader fade-in';
    preloader.appendChild(loader);
    container.appendChild(preloader);

    return { preloader, loader };
  }

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should hide the preloader after initializeLoader runs', fakeAsync(() => {
    const { preloader } = createPreloader();

    service.initializeLoader();
    tick(600);

    expect(preloader.style.display).toBe('none');
  }));

  it('should not set preloader display to block when dismissing', () => {
    const source = ReloadService.prototype.initializeLoader.toString();
    expect(source).not.toContain("display = 'block'");
  });

  it('should handle a missing preloader element gracefully', () => {
    expect(() => service.initializeLoader()).not.toThrow();
  });

  it('should hide preloader when only the preloader element exists', () => {
    const preloader = document.createElement('div');
    preloader.id = 'preloader';
    preloader.style.display = 'block';
    container.appendChild(preloader);

    service.initializeLoader();

    expect(preloader.style.display).toBe('none');
  });

  it('should show preloader again when resetLoader is called', () => {
    const { preloader, loader } = createPreloader();
    preloader.style.display = 'none';
    loader.classList.add('fade-out');

    service.resetLoader();

    expect(preloader.style.display).toBe('block');
    expect(loader.classList.contains('fade-in')).toBeTrue();
    expect(loader.classList.contains('fade-out')).toBeFalse();
  });
});
