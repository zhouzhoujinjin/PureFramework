import React, { Component } from 'react'
import PropTypes from 'prop-types'
import { EditorState, getDefaultKeyBinding, RichUtils } from 'draft-js'
import Editor from 'draft-js-plugins-editor'
import { stateToHTML } from 'draft-js-export-html'
import _ from 'lodash'
import createMentionPlugin, { defaultSuggestionsFilter } from 'draft-js-mention-plugin'
import 'draft-js-mention-plugin/lib/plugin.css'
import 'draft-js/dist/Draft.css'
import MentionComponent from './MentionComponent'
import EntryComponent from './EntryComponent'

const cmdState = {
  handled: 'handled',
  notHandled: 'not-handled'
}

class PostEditor extends Component {
  constructor(props) {
    super(props)
    this.mentionPlugin = createMentionPlugin({
      entityMutability: 'IMMUTABLE',
      mentionComponent: MentionComponent // since we want to remove the entire name at once.
    })
    this.state = {
      editorState: EditorState.createEmpty(),
      suggestions: this.props.mentions
    }
  }

  reset = () => {
    this.setState({
      editorState: EditorState.createEmpty()
    })
  }

  handleChange = (editorState) => {
    this.setState({
      editorState
    })
    this.props.onChange && this.props.onChange(this.toHtml())
  }

  toHtml = () => {
    const contentState = this.state.editorState.getCurrentContent()
    const options = {
      // eslint-disable-next-line consistent-return
      entityStyleFn: (entity) => {
        const entityType = entity.get('type').toLowerCase()
        if (entityType === 'mention') {
          const data = entity.getData()
          return {
            element: 'span',
            attributes: {
              'data-mention-id': _.get(data, 'mention.id'),
              class: 'mention_class'
            },
            style: {
              // Put styles here...
            }
          }
        }
      }
    }
    return stateToHTML(contentState, options)
  }

  onSearchChange = ({ value }) => {
    this.setState({
      suggestions: defaultSuggestionsFilter(value, this.props.mentions)
    })
  }

  keyBindingFn = (e) => {
    // retrun custom commands on keyPress if required
    return getDefaultKeyBinding(e)
  }

  handleKeyCommand = (command) => {
    // handle custom command here;

    const newState = RichUtils.handleKeyCommand(this.state.editorState, command)
    if (newState) {
      this.onChange(newState)
      return cmdState.handled
    }
    return cmdState.notHandled
  }

  render() {
    const { MentionSuggestions } = this.mentionPlugin
    const plugins = [this.mentionPlugin]
    const { className, style, placeholder } = this.props

    return (
      <div className={`editor ${className || ''}`} style={style}>
        <Editor
          editorState={this.state.editorState}
          onChange={this.handleChange}
          plugins={plugins}
          keyBindingFn={this.keyBindingFn}
          handleKeyCommand={this.handleKeyCommand}
          placeholder={placeholder}
          ref={(element) => {
            this.editor = element
          }}
        />

        <MentionSuggestions
          onSearchChange={this.onSearchChange}
          suggestions={this.state.suggestions}
          entryComponent={EntryComponent}
        />
      </div>
    )
  }
}

PostEditor.propTypes = {
  /**
   * mentions {array} - array of names for `@`mentions to work
   */
  mentions: PropTypes.arrayOf(
    PropTypes.shape({
      name: PropTypes.string,
      id: PropTypes.string
    })
  ),
  /**
   * className {string} - className applied to top most Wrapper
   */
  className: PropTypes.string,
  /**
   * style {object} - inline style to be applied to top most Wrapper
   */
  style: PropTypes.object,
  /**
   * placeholder {string} - placeholder to display when editor has no text
   */
  placeholder: PropTypes.string
}

PostEditor.defaultProps = {
  mentions: []
}

export default PostEditor
