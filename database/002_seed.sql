-- Demo user: demo@jobtracker.com / Demo123!
INSERT INTO users (id, name, email, password_hash) VALUES
    ('7b0e6a2c-3f1d-4c8e-9a5b-1d2e3f4a5b6c', 'Demo User', 'demo@jobtracker.com',
     'AQAAAAIAAYagAAAAEAN7/Z2pot+dfUhAnaU2rIcZgssgjd+B55pZQ9kRXQjF/3gEjbbs0B9GZoI+mYiP6Q==');

INSERT INTO job_applications (id, user_id, company, position, status, applied_on) VALUES
    ('a1c3e5f7-0b2d-4f6a-8c1e-3a5b7c9d1e01', '7b0e6a2c-3f1d-4c8e-9a5b-1d2e3f4a5b6c', 'Ballast Lane',    '.NET Developer',        'Interviewing', '2026-10-20'),
    ('a1c3e5f7-0b2d-4f6a-8c1e-3a5b7c9d1e02', '7b0e6a2c-3f1d-4c8e-9a5b-1d2e3f4a5b6c', 'Google',         'AI Engineer',      'Applied',      '2026-10-24'),
    ('a1c3e5f7-0b2d-4f6a-8c1e-3a5b7c9d1e03', '7b0e6a2c-3f1d-4c8e-9a5b-1d2e3f4a5b6c', 'Ford',        'Full Stack Developer',  'Offer',        '2026-10-02'),
    ('a1c3e5f7-0b2d-4f6a-8c1e-3a5b7c9d1e04', '7b0e6a2c-3f1d-4c8e-9a5b-1d2e3f4a5b6c', 'OpenAI', 'Software Engineer',     'Rejected',     '2026-09-28');
