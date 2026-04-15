import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';

import { Comments } from './comments';

describe('Comments', () => {
  let service: Comments;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient()]
    });
    service = TestBed.inject(Comments);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
