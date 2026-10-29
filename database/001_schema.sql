CREATE TABLE users (
    id            uuid         PRIMARY KEY,
    name          varchar(100) NOT NULL,
    email         text         NOT NULL UNIQUE,
    password_hash text         NOT NULL
);

CREATE TABLE job_applications (
    id         uuid         PRIMARY KEY,
    user_id    uuid         NOT NULL REFERENCES users (id),
    company    varchar(100) NOT NULL,
    position   varchar(100) NOT NULL,
    status     varchar(20)  NOT NULL CHECK (status IN ('Applied', 'Interviewing', 'Offer', 'Accepted', 'Rejected')),
    applied_on date         NOT NULL
);

CREATE INDEX ix_job_applications_user_id ON job_applications (user_id);
