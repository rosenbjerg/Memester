import { Component, h } from "preact";
import { FullThread, Thread } from "../../models";
import Loading from "../../components/Loading";
import ky from "ky";

interface Props {
    threadId: number;
}

interface State {
    thread?: FullThread;
}

export default class ThreadPage extends Component<Props, State> {
    componentDidMount() {
        ky.get(`/api/threads/${this.props.threadId}`)
            .json<FullThread>()
            .then(thread => this.setState({ thread }));
    }

    render(_: Props, { thread }: State) {
        if (thread === undefined) return <Loading />;

        return (
            <div>
                <h2>{thread.name}</h2>
                <ul>
                    {thread.memes.map(m => (
                        <li key={m.id}>{m.name}</li>
                    ))}
                </ul>
            </div>
        );
    }
}
