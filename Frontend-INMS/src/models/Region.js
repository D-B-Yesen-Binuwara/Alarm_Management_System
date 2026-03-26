/**
 * @typedef {Object} Region
 * @property {number} regionId
 * @property {string} name
 * @property {string} [description]
 */

export const regionModelSchema = {
  regionId: 'number',
  name: 'string',
  description: 'string (optional)'
};
