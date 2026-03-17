import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-placeholder-page',
  standalone: true,
  template: `
    <div class="min-h-[60vh] flex items-center justify-center p-8 bg-gray-50">
      <div class="bg-white border border-gray-200 rounded-xl shadow-sm px-8 py-10 text-center max-w-xl">
        <h1 class="text-2xl font-bold text-gray-800 mb-2">{{ title }}</h1>
        <p class="text-sm text-gray-500">
          This screen is prepared in the frontend and can be connected as soon as the corresponding backend behavior is available.
        </p>
      </div>
    </div>
  `
})
export class PlaceholderPageComponent {
  private readonly route = inject(ActivatedRoute);
  readonly title = this.route.snapshot.data['title'] ?? 'Coming Soon';
}
