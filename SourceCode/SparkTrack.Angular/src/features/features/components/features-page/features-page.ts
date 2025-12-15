import { Component, signal, ViewChild, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// Angular Material imports
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSortModule, MatSort, Sort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { SelectionModel } from '@angular/cdk/collections';
import { guid } from '../../../core/data/guid';
import { Feature } from '../../../core/data/feature';
import { Project } from '../../../core/data/project';

// Interface for Idea

// Interface for Channel

@Component({
  selector: 'app-features-page',
  imports: [
    CommonModule,
    FormsModule,

    // Material Modules
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatCheckboxModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatSortModule,
  ],
  templateUrl: './features-page.html',
  styleUrls: ['./features-page.scss'],
})
export class FeaturesPage {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Signals
  showCompleted = signal(true);
  channelFilterEnabled = signal(false);
  selectedProject = signal<guid | null>(null);

  // Selection model for checkboxes
  selection = new SelectionModel<Feature>(true, []);

  // Pagination signals
  pageSize = signal(5);
  pageSizeOptions = signal([5, 10, 25]);
  currentPage = signal(0);

  // Mock data
  ideas = signal<Feature[]>([
    {
      id: 1,
      name: 'Новый дизайн главной страницы',
      deadline: new Date('2024-02-15'),
      status: 'В работе',
      project: 'Веб',
    },
    {
      id: 2,
      name: 'Мобильное приложение для iOS',
      deadline: new Date('2024-03-01'),
      status: 'Запланировано',
      project: 'Мобильное',
    },
    {
      id: 3,
      name: 'Интеграция с Telegram API',
      deadline: new Date('2024-01-30'),
      status: 'Завершено',
      project: 'Бэкенд',
    },
    {
      id: 4,
      name: 'Оптимизация базы данных',
      deadline: new Date('2024-02-28'),
      status: 'В работе',
      project: 'Бэкенд',
    },
    {
      id: 5,
      name: 'Адаптивная верстка каталога',
      deadline: new Date('2024-02-10'),
      status: 'Завершено',
      project: 'Веб',
    },
    {
      id: 6,
      name: 'Push-уведомления в Android',
      deadline: new Date('2024-03-15'),
      status: 'Запланировано',
      project: 'Мобильное',
    },
    {
      id: 7,
      name: 'Система кэширования',
      deadline: new Date('2024-02-20'),
      status: 'В работе',
      project: 'Бэкенд',
    },
    {
      id: 8,
      name: 'Детальная аналитика пользователей',
      deadline: new Date('2024-03-05'),
      status: 'Запланировано',
      project: 'Аналитика',
    },
    {
      id: 9,
      name: 'Обновление UI компонентов',
      deadline: new Date('2024-02-25'),
      status: 'В работе',
      project: 'Веб',
    },
    {
      id: 10,
      name: 'Микросервис авторизации',
      deadline: new Date('2024-03-10'),
      status: 'Запланировано',
      project: 'Бэкенд',
    },
    {
      id: 11,
      name: 'Автоматическое тестирование',
      deadline: new Date('2024-02-18'),
      status: 'Завершено',
      project: 'QA',
    },
    {
      id: 12,
      name: 'Система мониторинга',
      deadline: new Date('2024-03-20'),
      status: 'Запланировано',
      project: 'DevOps',
    },
  ]);

  channels = signal<Project[]>([
    { id: '1', name: 'Веб' },
    { id: '2', name: 'Мобильное' },
    { id: '3', name: 'Бэкенд' },
    { id: '4', name: 'Фронтенд' },
    { id: '5', name: 'Дизайн' },
    { id: '6', name: 'Аналитика' },
    { id: '7', name: 'QA' },
    { id: '8', name: 'DevOps' },
  ]);

  // Table columns - добавлен столбец select
  displayedColumns = signal<string[]>(['select', 'id', 'title', 'deadline', 'status']);

  // Computed values
  filteredIdeas = computed(() => {
    const ideas = this.ideas();
    const showCompleted = this.showCompleted();
    const channelFilterEnabled = this.channelFilterEnabled();
    const selectedChannelId = this.selectedProject();

    let filtered = [...ideas];

    // Filter by completed status
    if (!showCompleted) {
      filtered = filtered.filter((idea) => idea.status !== 'Завершено');
    }

    // Filter by channel
    if (channelFilterEnabled && selectedChannelId !== null) {
      const selectedChannel = this.channels().find((ch) => ch.id === selectedChannelId);
      if (selectedChannel) {
        filtered = filtered.filter((idea) => idea.project === selectedChannel.name);
      }
    }

    // Apply sorting
    return this.sortData(filtered);
  });

  // Computed для пагинированных данных
  paginatedIdeas = computed(() => {
    const filtered = this.filteredIdeas();
    const startIndex = this.currentPage() * this.pageSize();
    const endIndex = startIndex + this.pageSize();
    return filtered.slice(startIndex, endIndex);
  });

  // Current sort state
  sortState = signal<Sort>({ active: 'id', direction: 'asc' });

  constructor() {
    // Update displayed columns based on channel filter
    this.updateDisplayedColumns();
  }

  private updateDisplayedColumns(): void {
    const columns = ['select', 'id', 'title', 'deadline'];

    if (!this.channelFilterEnabled()) {
      columns.push('channel');
    }

    columns.push('status');
    this.displayedColumns.set(columns);
  }

  onShowCompletedChange(): void {
    this.selection.clear();
    this.currentPage.set(0);
  }

  onChannelFilterEnabledChange(): void {
    if (!this.channelFilterEnabled()) {
      this.selectedProject.set(null);
    }
    this.updateDisplayedColumns();
    this.selection.clear();
    this.currentPage.set(0);
  }

  onProjectSelectionChange(): void {
    this.selection.clear();
    this.currentPage.set(0);
  }

  // Sorting logic
  sortData(data: Feature[]): Feature[] {
    const sort = this.sortState();
    if (!sort.active || sort.direction === '') {
      return data;
    }

    return data.sort((a, b) => {
      const isAsc = sort.direction === 'asc';
      switch (sort.active) {
        case 'id':
          return compare(a.id, b.id, isAsc);
        case 'title':
          return compare(a.name, b.name, isAsc);
        case 'deadline':
          return compare(a.deadline.getTime(), b.deadline.getTime(), isAsc);
        case 'channel':
          return compare(a.project, b.project, isAsc);
        case 'status':
          return compare(a.status, b.status, isAsc);
        default:
          return 0;
      }
    });
  }

  onSortChange(sort: Sort): void {
    this.sortState.set(sort);
    this.currentPage.set(0);
  }

  // Row click handler
  onRowClick(idea: Feature): void {
    console.log('Row clicked:', idea);
    // Здесь можно добавить навигацию на детальную страницу идеи
    // this.router.navigate(['/idea', idea.id]);
  }

  // Pagination handlers
  onPageChange(event: PageEvent): void {
    this.currentPage.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.selection.clear(); // Очищаем выделение при смене страницы
  }

  // Checkbox selection methods
  isAllSelected(): boolean {
    const numSelected = this.selection.selected.length;
    const numRows = this.paginatedIdeas().length;
    return numSelected === numRows && numRows > 0;
  }

  toggleAllRows(): void {
    if (this.isAllSelected()) {
      this.selection.clear();
    } else {
      this.paginatedIdeas().forEach((row) => this.selection.select(row));
    }
  }

  checkboxLabel(row?: Feature): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.id}`;
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Завершено':
        return 'status-completed';
      case 'В работе':
        return 'status-in-progress';
      case 'Запланировано':
        return 'status-planned';
      default:
        return 'status-default';
    }
  }
}

// Helper function for sorting
function compare(a: number | string | Date, b: number | string | Date, isAsc: boolean): number {
  const aValue = a instanceof Date ? a.getTime() : a;
  const bValue = b instanceof Date ? b.getTime() : b;

  return (aValue < bValue ? -1 : 1) * (isAsc ? 1 : -1);
}
