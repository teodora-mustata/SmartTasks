import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';

import { Lists } from './lists';

describe('Lists', () => {
  let service: Lists;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient()]
    });
    service = TestBed.inject(Lists);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
