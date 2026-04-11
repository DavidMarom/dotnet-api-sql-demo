CREATE TABLE companies (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL
);

CREATE TABLE contact_companies (
    contact_id INT NOT NULL REFERENCES contacts(id) ON DELETE CASCADE,
    company_id INT NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    PRIMARY KEY (contact_id, company_id)
);
